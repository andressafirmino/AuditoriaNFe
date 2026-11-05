using AuditoriaNFe.Tests.Arquitetura;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace AuditoriaNFe.Tests.Integracao.Infra;

/// <summary>
/// PostgreSQL descartável com o script de <c>infra/postgres/init</c>. A conexão usa o usuário da
/// aplicação, sem privilégios de administrador, como no ambiente local.
/// </summary>
public sealed class ContainerPostgres : IAsyncLifetime
{
    public const string Imagem = "pgvector/pgvector:0.8.7-pg17";
    private const int Porta = 5432;

    private readonly string _usuarioApp = $"app{Guid.NewGuid():N}"[..12];
    private readonly string _senhaApp = Guid.NewGuid().ToString("N");
    private readonly IContainer _container;

    public ContainerPostgres()
    {
        _container = new ContainerBuilder(Imagem)
            .WithEnvironment("POSTGRES_DB", "auditoria_agente")
            .WithEnvironment("POSTGRES_USER", "administrador")
            .WithEnvironment("POSTGRES_PASSWORD", Guid.NewGuid().ToString("N"))
            .WithEnvironment("POSTGRES_APP_USER", _usuarioApp)
            .WithEnvironment("POSTGRES_APP_PASSWORD", _senhaApp)
            .WithResourceMapping(new DirectoryInfo(Path.Combine(Repositorio.Raiz(), "infra", "postgres", "init")), "/docker-entrypoint-initdb.d/")
            .WithPortBinding(Porta, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("PostgreSQL init process complete")
                .UntilCommandIsCompleted("pg_isready", "-h", "127.0.0.1", "-U", "administrador", "-d", "auditoria_agente"))
            .Build();
    }

    public string ConnectionString =>
        $"Host={_container.Hostname};Port={_container.GetMappedPublicPort(Porta)};Database=auditoria_agente;Username={_usuarioApp};Password={_senhaApp}";

    public async ValueTask InitializeAsync() => await LogContainer.IniciarAsync(_container, Imagem);

    public Task PararAsync() => _container.StopAsync();

    public Task IniciarAsync() => _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
