using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Infrastructure.Persistencia.Mongo;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using AuditoriaNFe.Infrastructure.Saude;
using AuditoriaNFe.Tests.Integracao.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Tests.Integracao;

public sealed class SaudeTests(ContainerMongo mongo, ContainerPostgres postgres)
    : IClassFixture<ContainerMongo>, IClassFixture<ContainerPostgres>
{
    private static readonly IOptions<PersistenciaOpcoes> Persistencia = Options.Create(
        new PersistenciaOpcoes { TimeoutMongo = TimeSpan.FromSeconds(2), TimeoutPostgres = TimeSpan.FromSeconds(2) });

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    [Fact]
    public async Task MongoDB_disponivel_e_saudavel()
    {
        var resultado = await Verificar(new VerificacaoMongo(new ContextoMongo(mongo.ConnectionString, Persistencia)));

        Assert.Equal(HealthStatus.Healthy, resultado.Status);
    }

    [Fact]
    public async Task MongoDB_inacessivel_e_falha_sem_detalhes_dentro_do_timeout()
    {
        var inicio = DateTime.UtcNow;

        var resultado = await Verificar(new VerificacaoMongo(new ContextoMongo("mongodb://127.0.0.1:1/auditoria_agente", Persistencia)));

        Assert.True(DateTime.UtcNow - inicio < TimeSpan.FromSeconds(10));
        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
        Assert.Equal("MongoDB indisponível.", resultado.Description);
        Assert.Null(resultado.Exception);
    }

    [Fact]
    public async Task PostgreSQL_migrado_e_saudavel()
    {
        var fabrica = new FabricaContexto(postgres.ConnectionString);
        var verificacao = new VerificacaoPostgres(fabrica, Persistencia);

        await using (var contexto = fabrica.CreateDbContext())
        {
            await contexto.Database.MigrateAsync(Cancelamento);
        }

        Assert.Equal(HealthStatus.Healthy, (await Verificar(verificacao)).Status);
    }

    [Fact]
    public async Task PostgreSQL_inacessivel_e_falha_sem_detalhes()
    {
        var resultado = await Verificar(new VerificacaoPostgres(
            new FabricaContexto("Host=127.0.0.1;Port=1;Database=auditoria_agente;Username=teste"), Persistencia));

        Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
        Assert.Equal("PostgreSQL indisponível.", resultado.Description);
        Assert.Null(resultado.Exception);
    }

    [Fact]
    public async Task Diretorio_de_aprovacao_e_criado_e_fica_vazio_apos_a_verificacao()
    {
        var diretorio = Path.Combine(Path.GetTempPath(), $"aprovacoes-{Guid.NewGuid():N}");
        try
        {
            var resultado = await Verificar(new VerificacaoDiretorioAprovacao(Options.Create(new AprovacaoOpcoes { Diretorio = diretorio })));

            Assert.Equal(HealthStatus.Healthy, resultado.Status);
            Assert.Empty(Directory.EnumerateFileSystemEntries(diretorio));
        }
        finally
        {
            Directory.Delete(diretorio, recursive: true);
        }
    }

    [Fact]
    public async Task Diretorio_de_aprovacao_nao_gravavel_falha_sem_revelar_o_caminho()
    {
        var arquivo = Path.GetTempFileName();
        try
        {
            var resultado = await Verificar(new VerificacaoDiretorioAprovacao(Options.Create(new AprovacaoOpcoes { Diretorio = arquivo })));

            Assert.Equal(HealthStatus.Unhealthy, resultado.Status);
            Assert.DoesNotContain(Path.GetFileName(arquivo), resultado.Description, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(arquivo);
        }
    }

    private static Task<HealthCheckResult> Verificar(IHealthCheck verificacao) =>
        verificacao.CheckHealthAsync(
            new HealthCheckContext { Registration = new HealthCheckRegistration("teste", verificacao, HealthStatus.Unhealthy, null) },
            Cancelamento);

    private sealed class FabricaContexto(string connectionString) : IDbContextFactory<ConsumoDbContext>
    {
        public ConsumoDbContext CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<ConsumoDbContext>();
            ConsumoDbContext.Configurar(builder, connectionString, Persistencia.Value);
            return new ConsumoDbContext(builder.Options);
        }
    }
}
