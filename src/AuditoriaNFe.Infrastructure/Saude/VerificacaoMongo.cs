using AuditoriaNFe.Infrastructure.Persistencia.Mongo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace AuditoriaNFe.Infrastructure.Saude;

/// <summary>MongoDB acessível pelo usuário da aplicação, dentro de <c>Persistencia:TimeoutMongo</c> (R-21).</summary>
public sealed class VerificacaoMongo(ContextoMongo contexto) : IHealthCheck
{
    public const string Nome = "mongodb";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await contexto.ExecutarAsync(
                token => contexto.Banco.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: token),
                cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception erro) when (erro is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "MongoDB indisponível.");
        }
    }
}
