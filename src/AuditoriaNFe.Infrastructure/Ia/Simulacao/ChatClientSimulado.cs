using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AuditoriaNFe.Infrastructure.Ia.Simulacao;

/// <summary>
/// Provedor de IA simulado para uso local com <c>Ia:Provedor=Simulado</c> (plan.md, Structure
/// Decision). Não consulta nenhum modelo: devolve um conteúdo fixo que se declara simulado, com
/// <see cref="ChatResponse.ModelId"/> <c>simulado</c> e sem <see cref="ChatResponse.Usage"/>, para que
/// nenhum dado simulado seja apresentado como real. Fora de <c>Development</c> é recusado.
/// </summary>
public sealed class ChatClientSimulado : IChatClient
{
    public const string NomeProvedor = "Simulado";
    public const string Modelo = "simulado";
    public const string AmbientePermitido = "Development";
    public const string Mensagem = "Resposta simulada: nenhum provedor de IA foi consultado.";

    private static readonly ChatClientMetadata Metadados = new(NomeProvedor, defaultModelId: Modelo);

    public ChatClientSimulado(string ambiente)
    {
        if (!string.Equals(ambiente, AmbientePermitido, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"O provedor de IA simulado só é aceito no ambiente {AmbientePermitido}; configure Ia:Provedor=OpenAI.");
        }
    }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var texto = options?.ResponseFormat is ChatResponseFormatJson
            ? $$"""{"simulado":true,"mensagem":"{{Mensagem}}"}"""
            : Mensagem;

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, texto))
        {
            ModelId = Modelo,
            FinishReason = ChatFinishReason.Stop,
        });
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resposta = await GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
        foreach (var atualizacao in resposta.ToChatResponseUpdates())
        {
            yield return atualizacao;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is not null ? null
        : serviceType == typeof(ChatClientMetadata) ? Metadados
        : serviceType.IsInstanceOfType(this) ? this
        : null;

    public void Dispose()
    {
    }
}
