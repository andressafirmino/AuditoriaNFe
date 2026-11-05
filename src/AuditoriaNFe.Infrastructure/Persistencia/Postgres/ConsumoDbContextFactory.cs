using AuditoriaNFe.Application.Configuracao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuditoriaNFe.Infrastructure.Persistencia.Postgres;

/// <summary>Só para <c>dotnet ef migrations</c>: gera o modelo sem conectar ao banco.</summary>
internal sealed class ConsumoDbContextFactory : IDesignTimeDbContextFactory<ConsumoDbContext>
{
    public ConsumoDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ConsumoDbContext>();
        ConsumoDbContext.Configurar(
            builder,
            "Host=localhost;Database=auditoria_agente",
            new PersistenciaOpcoes { TimeoutMongo = TimeSpan.FromSeconds(5), TimeoutPostgres = TimeSpan.FromSeconds(5) });
        return new ConsumoDbContext(builder.Options);
    }
}
