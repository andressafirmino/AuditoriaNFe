using System.Runtime.CompilerServices;
using AuditoriaNFe.Domain.Sessoes;
using Microsoft.Extensions.AI;

namespace AuditoriaNFe.Infrastructure.Ia;

/// <summary>
/// Registra cada chamada ao provedor, inclusive cada volta do loop de ferramentas e cada retentativa
/// (research R-09, R-16; P14). Tokens vêm de <see cref="ChatResponse.Usage"/> e ficam nulos quando o
/// provedor não os informa. O registro nunca altera a resposta nem a exceção do provedor.
/// </summary>
public sealed class RegistroConsumoChatClient(
    IChatClient interno,
    GravadorConsumo gravador,
    string provedor,
    OrigemValoresConsumo origem) : DelegatingChatClient(interno)
{
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var medicao = gravador.IniciarMedicao();
        ChatResponse? resposta = null;
        var status = StatusChamadaProvedor.Falha;
        try
        {
            resposta = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            status = StatusChamadaProvedor.Sucesso;
            return resposta;
        }
        catch (Exception erro) when (erro is OperationCanceledException or TimeoutException)
        {
            status = StatusChamadaProvedor.Timeout;
            throw;
        }
        finally
        {
            await gravador.RegistrarChatAsync(
                medicao, provedor, resposta?.ModelId ?? ModeloSolicitado(options), origem, status, resposta?.Usage)
                .ConfigureAwait(false);
        }
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var medicao = gravador.IniciarMedicao();
        UsageDetails? uso = null;
        string? modelo = null;
        var concluida = false;
        try
        {
            await foreach (var atualizacao in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
            {
                modelo ??= atualizacao.ModelId;
                foreach (var conteudo in atualizacao.Contents.OfType<UsageContent>())
                {
                    uso ??= new UsageDetails();
                    uso.Add(conteudo.Details);
                }

                yield return atualizacao;
            }

            concluida = true;
        }
        finally
        {
            var status = concluida ? StatusChamadaProvedor.Sucesso
                : cancellationToken.IsCancellationRequested ? StatusChamadaProvedor.Timeout
                : StatusChamadaProvedor.Falha;
            await gravador.RegistrarChatAsync(medicao, provedor, modelo ?? ModeloSolicitado(options), origem, status, uso)
                .ConfigureAwait(false);
        }
    }

    private string? ModeloSolicitado(ChatOptions? options) =>
        options?.ModelId ?? InnerClient.GetService<ChatClientMetadata>()?.DefaultModelId;
}
