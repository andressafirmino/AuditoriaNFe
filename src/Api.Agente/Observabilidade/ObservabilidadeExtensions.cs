using AuditoriaNFe.Application.Consumo;
using AuditoriaNFe.Infrastructure.Ia;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Agente.Observabilidade;

/// <summary>
/// OpenTelemetry para traces, métricas e logs (ADR-0009, research R-22). Nenhum conteúdo de XML,
/// pergunta, resposta, contexto do usuário ou <c>usuarioId</c> é registrado; a exportação OTLP só é
/// ativada com <c>OTEL_EXPORTER_OTLP_ENDPOINT</c>.
/// </summary>
public static class ObservabilidadeExtensions
{
    public const string NomeServico = "Api.Agente";

    public static IServiceCollection AdicionarObservabilidade(this IServiceCollection services, IConfiguration configuration)
    {
        var telemetria = services.AddOpenTelemetry()
            .ConfigureResource(recurso => recurso.AddService(NomeServico))
            .WithTracing(traces => traces
                .AddAspNetCoreInstrumentation()
                .AddSource(FabricaChatClient.NomeFonteTelemetria))
            .WithMetrics(metricas => metricas
                .AddAspNetCoreInstrumentation()
                .AddMeter(GravadorConsumo.NomeMedidor, FabricaChatClient.NomeFonteTelemetria))
            .WithLogging(
                logs => logs.AddProcessor(new EnriquecedorLogExecucao()),
                opcoes =>
                {
                    opcoes.IncludeScopes = true;
                    opcoes.IncludeFormattedMessage = true;
                });

        if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            telemetria.UseOtlpExporter();
        }

        return services;
    }
}

/// <summary>Acrescenta <c>execucaoId</c> e <c>correlacaoId</c> da execução corrente a cada log.</summary>
public sealed class EnriquecedorLogExecucao : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        var escopo = EscopoConsumo.Atual;
        if (escopo is null)
        {
            return;
        }

        var atributos = new List<KeyValuePair<string, object?>>(data.Attributes ?? [])
        {
            new("execucaoId", escopo.ExecucaoId),
            new("correlacaoId", escopo.CorrelacaoId),
        };
        data.Attributes = atributos;
    }
}
