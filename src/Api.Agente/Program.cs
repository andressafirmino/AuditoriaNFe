using Api.Agente.Documentacao;
using Api.Agente.Erros;
using Api.Agente.Limites;
using Api.Agente.Observabilidade;
using Api.Agente.Seguranca;
using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Application.Contratos;
using AuditoriaNFe.Infrastructure;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using AuditoriaNFe.Infrastructure.Saude;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

GuardaDesvioD05.Verificar(builder.Environment, builder.Configuration);

builder.Services
    .AdicionarOpcoesAuditoria(builder.Configuration)
    .AdicionarComum()
    .AdicionarPersistenciaMongo(builder.Configuration)
    .AdicionarPersistenciaConsumo(builder.Configuration)
    .AdicionarIa(builder.Environment.EnvironmentName)
    .AdicionarObservabilidade(builder.Configuration)
    .AdicionarLimitesHttp();

builder.Services.ConfigureHttpJsonOptions(opcoes => OpcoesJsonContrato.Aplicar(opcoes.SerializerOptions));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorExcecoes>();

const string Prontidao = "ready";
builder.Services.AddHealthChecks()
    .AddCheck<VerificacaoMongo>(VerificacaoMongo.Nome, tags: [Prontidao])
    .AddCheck<VerificacaoPostgres>(VerificacaoPostgres.Nome, tags: [Prontidao])
    .AddCheck<VerificacaoDiretorioAprovacao>(VerificacaoDiretorioAprovacao.Nome, tags: [Prontidao]);

builder.Services.AdicionarDocumentacaoApi(builder.Environment);

var app = builder.Build();

await MigracoesConsumo.AplicarSeHabilitadoAsync(app.Services);

app.UseExceptionHandler();
app.UsarDocumentacaoApi();
app.UseRouting();
app.UseRateLimiter();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = registro => registro.Tags.Contains(Prontidao) });

await app.RunAsync();

public partial class Program;
