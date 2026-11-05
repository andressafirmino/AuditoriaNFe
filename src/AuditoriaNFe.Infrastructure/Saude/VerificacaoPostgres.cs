using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Infrastructure.Saude;

/// <summary>
/// PostgreSQL acessível e tabela <c>consumo.chamadas_provedor</c> legível pelo usuário da aplicação,
/// dentro de <c>Persistencia:TimeoutPostgres</c> (R-21).
/// </summary>
public sealed class VerificacaoPostgres(IDbContextFactory<ConsumoDbContext> fabrica, IOptions<PersistenciaOpcoes> opcoes) : IHealthCheck
{
    public const string Nome = "postgresql";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(opcoes.Value.TimeoutPostgres ?? TimeSpan.FromSeconds(5));
        try
        {
            await using var contexto = await fabrica.CreateDbContextAsync(limite.Token).ConfigureAwait(false);
            await contexto.Database
                .ExecuteSqlRawAsync("SELECT 1 FROM consumo.chamadas_provedor LIMIT 0", limite.Token)
                .ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception erro) when (erro is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "PostgreSQL indisponível.");
        }
    }
}
