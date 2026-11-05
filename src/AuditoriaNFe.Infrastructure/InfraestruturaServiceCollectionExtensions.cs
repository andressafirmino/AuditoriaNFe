using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Infrastructure.Comum;
using AuditoriaNFe.Infrastructure.Persistencia.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Infrastructure;

/// <summary>Composição da Infraestrutura usada pelo host.</summary>
public static class InfraestruturaServiceCollectionExtensions
{
    public static IServiceCollection AdicionarComum(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IRelogio, RelogioSistema>();
        services.TryAddSingleton<IGeradorIds, GeradorIdsV7>();
        return services;
    }

    public static IServiceCollection AdicionarPersistenciaMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(provedor => new ContextoMongo(
            configuration.GetConnectionString(ContextoMongo.NomeConnectionString)
                ?? throw new InvalidOperationException($"ConnectionStrings:{ContextoMongo.NomeConnectionString} é obrigatória."),
            provedor.GetRequiredService<IOptions<PersistenciaOpcoes>>()));
        services.AddSingleton<ISessaoRepositorio, SessaoRepositorio>();
        services.AddSingleton<IExecucaoRepositorio, ExecucaoRepositorio>();
        services.AddSingleton<IDocumentoRepositorio, DocumentoRepositorio>();
        return services;
    }
}
