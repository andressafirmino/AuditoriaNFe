using System.ClientModel;
using System.ClientModel.Primitives;
using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Domain.Sessoes;
using AuditoriaNFe.Infrastructure.Ia.Simulacao;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AuditoriaNFe.Infrastructure.Ia;

/// <summary>
/// Monta o <see cref="IChatClient"/> da etapa 7 (research R-03, R-09): OpenAI por Chat Completions
/// com <c>store=false</c>, sem retentativas automáticas do SDK (cada chamada paga passa pelo registro
/// de consumo) e com OpenTelemetry sem dados sensíveis; ou o provedor simulado, só em
/// <c>Development</c>.
/// </summary>
public sealed class FabricaChatClient(
    IOptions<IaOpcoes> ia,
    IOptions<OpenAIOpcoes> openAi,
    GravadorConsumo gravador,
    ILoggerFactory logs,
    string ambiente)
{
    public const string NomeFonteTelemetria = "AuditoriaNFe.Ia";
    public const string NomeProvedorOpenAI = "OpenAI";

    public IChatClient Criar()
    {
        var (provedor, nomeProvedor, origem) = ia.Value.Provedor switch
        {
            ProvedorIa.OpenAI => (CriarOpenAI(), NomeProvedorOpenAI, OrigemValoresConsumo.InformadoProvedor),
            ProvedorIa.Simulado => ((IChatClient)new ChatClientSimulado(ambiente), ChatClientSimulado.NomeProvedor, OrigemValoresConsumo.EstimadoSistema),
            _ => throw new InvalidOperationException("Ia:Provedor é obrigatório."),
        };

        return Montar(provedor, nomeProvedor, origem);
    }

    /// <summary>Envolve o provedor com telemetria, registro de consumo e <c>store=false</c>.</summary>
    public IChatClient Montar(IChatClient provedor, string nomeProvedor, OrigemValoresConsumo origem)
    {
        ArgumentNullException.ThrowIfNull(provedor);
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeProvedor);

        return new ChatClientBuilder(provedor)
            .UseOpenTelemetry(logs, NomeFonteTelemetria, telemetria => telemetria.EnableSensitiveData = false)
            .Use(interno => new RegistroConsumoChatClient(interno, gravador, nomeProvedor, origem))
            .ConfigureOptions(AplicarSemArmazenamento)
            .Build();
    }

    private IChatClient CriarOpenAI()
    {
        var opcoes = openAi.Value;
        if (string.IsNullOrWhiteSpace(opcoes.ApiKey) || string.IsNullOrWhiteSpace(opcoes.Modelo))
        {
            throw new InvalidOperationException("OpenAI:ApiKey e OpenAI:Modelo são obrigatórios com Ia:Provedor=OpenAI.");
        }

        var cliente = new OpenAIClient(new ApiKeyCredential(opcoes.ApiKey), new OpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
            NetworkTimeout = ia.Value.TimeoutChamada,
        });

        return cliente.GetChatClient(opcoes.Modelo).AsIChatClient();
    }

    /// <summary>Preserva as opções nativas já definidas e força <c>store=false</c> (R-03).</summary>
    public static void AplicarSemArmazenamento(ChatOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        var anterior = opcoes.RawRepresentationFactory;
        opcoes.RawRepresentationFactory = cliente =>
        {
            var nativas = anterior?.Invoke(cliente) as ChatCompletionOptions ?? new ChatCompletionOptions();
            nativas.StoredOutputEnabled = false;
            return nativas;
        };
    }
}
