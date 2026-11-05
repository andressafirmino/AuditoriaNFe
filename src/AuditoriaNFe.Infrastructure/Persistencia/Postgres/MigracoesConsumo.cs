using AuditoriaNFe.Application.Configuracao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Infrastructure.Persistencia.Postgres;

/// <summary>Aplica as migrações do schema <c>consumo</c> só com <c>Persistencia:AplicarMigracoes=true</c> (R-16).</summary>
public static class MigracoesConsumo
{
    public static async Task AplicarSeHabilitadoAsync(IServiceProvider servicos, CancellationToken cancellationToken = default)
    {
        if (!servicos.GetRequiredService<IOptions<PersistenciaOpcoes>>().Value.AplicarMigracoes)
        {
            return;
        }

        var fabrica = servicos.GetRequiredService<IDbContextFactory<ConsumoDbContext>>();
        await using var contexto = await fabrica.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await contexto.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }
}
