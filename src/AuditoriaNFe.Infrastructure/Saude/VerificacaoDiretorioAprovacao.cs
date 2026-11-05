using AuditoriaNFe.Application.Configuracao;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Infrastructure.Saude;

/// <summary>
/// Diretório <c>Aprovacao:Diretorio</c> gravável: cria o diretório se faltar e grava e remove um
/// arquivo temporário (R-21). A resposta não revela o caminho.
/// </summary>
public sealed class VerificacaoDiretorioAprovacao(IOptions<AprovacaoOpcoes> opcoes) : IHealthCheck
{
    public const string Nome = "diretorio-aprovacao";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var diretorio = Path.GetFullPath(opcoes.Value.Diretorio);
        var arquivo = Path.Combine(diretorio, $".saude-{Guid.NewGuid():N}.tmp");
        try
        {
            Directory.CreateDirectory(diretorio);
            await File.WriteAllBytesAsync(arquivo, [1], cancellationToken).ConfigureAwait(false);
            File.Delete(arquivo);
            return HealthCheckResult.Healthy();
        }
        catch (Exception erro) when (erro is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Diretório de aprovações sem permissão de gravação.");
        }
    }
}
