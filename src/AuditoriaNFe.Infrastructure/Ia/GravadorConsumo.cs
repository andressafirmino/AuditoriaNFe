using System.Diagnostics;
using System.Diagnostics.Metrics;
using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Application.Consumo;
using AuditoriaNFe.Domain.Sessoes;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Infrastructure.Ia;

/// <summary>Status gravado em <c>consumo.chamadas_provedor.status</c>.</summary>
public enum StatusChamadaProvedor
{
    Sucesso,
    Falha,
    Timeout,
    RespostaInvalida,
}

/// <summary>Instante e marca de tempo do início de uma chamada ao provedor.</summary>
public readonly record struct MedicaoChamada(DateTimeOffset IniciadaEm, long MarcaInicio);

/// <summary>
/// Grava uma linha de <c>consumo.chamadas_provedor</c> por chamada em "melhor esforço" (research
/// R-16): a falha é registrada em log e métrica, marca o resumo da execução como incompleto e nunca
/// chega a quem chamou o provedor. A gravação usa <c>Persistencia:TimeoutPostgres</c>, e não o token
/// da chamada, para que a chamada cancelada também seja registrada.
/// </summary>
public sealed partial class GravadorConsumo
{
    public const string NomeMedidor = "AuditoriaNFe.Consumo";
    public const string NomeContadorFalhas = "auditoria_nfe.consumo.falhas_registro";

    private readonly IDbContextFactory<ConsumoDbContext> _fabrica;
    private readonly CalculadoraCusto _calculadora;
    private readonly IRelogio _relogio;
    private readonly IGeradorIds _ids;
    private readonly TimeSpan _timeout;
    private readonly ILogger<GravadorConsumo> _logger;
    private readonly Counter<long> _falhas;

    public GravadorConsumo(
        IDbContextFactory<ConsumoDbContext> fabrica,
        CalculadoraCusto calculadora,
        IRelogio relogio,
        IGeradorIds ids,
        IOptions<PersistenciaOpcoes> persistencia,
        IMeterFactory medidores,
        ILogger<GravadorConsumo> logger)
    {
        _fabrica = fabrica;
        _calculadora = calculadora;
        _relogio = relogio;
        _ids = ids;
        _timeout = persistencia.Value.TimeoutPostgres
            ?? throw new InvalidOperationException("Persistencia:TimeoutPostgres é obrigatório.");
        _logger = logger;
        _falhas = medidores.Create(NomeMedidor).CreateCounter<long>(
            NomeContadorFalhas, description: "Chamadas ao provedor que não puderam ser registradas.");
    }

    public MedicaoChamada IniciarMedicao() => new(_relogio.Agora, Stopwatch.GetTimestamp());

    public async Task RegistrarChatAsync(
        MedicaoChamada medicao,
        string provedor,
        string? modelo,
        OrigemValoresConsumo origem,
        StatusChamadaProvedor status,
        UsageDetails? uso)
    {
        var duracao = Stopwatch.GetElapsedTime(medicao.MarcaInicio);
        var escopo = EscopoConsumo.Atual;
        if (escopo is null)
        {
            _falhas.Add(1, new KeyValuePair<string, object?>("motivo", "sem_contexto"));
            LogSemContexto(_logger);
            return;
        }

        var tokensEntrada = ParaInt(uso?.InputTokenCount);
        var tokensCache = ParaInt(uso?.CachedInputTokenCount);
        var tokensSaida = ParaInt(uso?.OutputTokenCount);
        var modeloEfetivo = string.IsNullOrWhiteSpace(modelo) ? "desconhecido" : modelo;
        var custo = _calculadora.Calcular(modeloEfetivo, tokensEntrada, tokensCache, tokensSaida);

        escopo.RegistrarChamada(new ConsumoChamada(tokensEntrada, tokensCache, tokensSaida, custo.Total, custo.Moeda, origem));

        var linha = new ChamadaProvedor
        {
            Id = _ids.Novo(),
            ExecucaoId = escopo.ExecucaoId,
            CorrelacaoId = escopo.CorrelacaoId,
            Operacao = escopo.Operacao,
            TipoChamada = "Chat",
            Provedor = provedor,
            Modelo = modeloEfetivo,
            Tentativa = escopo.Tentativa,
            TokensEntrada = tokensEntrada,
            TokensEntradaCache = tokensCache,
            TokensSaida = tokensSaida,
            IniciadaEm = medicao.IniciadaEm,
            DuracaoMs = (int)Math.Min(int.MaxValue, Math.Max(0, duracao.TotalMilliseconds)),
            Status = status.ToString(),
            CustoEntrada = custo.Entrada,
            CustoSaida = custo.Saida,
            CustoTotal = custo.Total,
            Moeda = custo.Moeda,
            VersaoTabelaPrecos = custo.VersaoTabela,
            OrigemValores = origem.ToString(),
            ChaveIsolamento = escopo.ChaveIsolamento,
        };

        try
        {
            using var limite = new CancellationTokenSource(_timeout);
            await using var contexto = await _fabrica.CreateDbContextAsync(limite.Token).ConfigureAwait(false);
            contexto.ChamadasProvedor.Add(linha);
            await contexto.SaveChangesAsync(limite.Token).ConfigureAwait(false);
        }
        catch (Exception erro) when (erro is not OutOfMemoryException)
        {
            escopo.RegistrarFalhaGravacao();
            _falhas.Add(1, new KeyValuePair<string, object?>("motivo", "gravacao"));
            LogFalhaGravacao(_logger, erro.GetType().Name, escopo.ExecucaoId);
        }
    }

    private static int? ParaInt(long? valor) => valor is >= 0 and <= int.MaxValue ? (int)valor.Value : null;

    [LoggerMessage(EventId = 1401, Level = LogLevel.Warning,
        Message = "Chamada ao provedor sem contexto de execução; consumo não registrado.")]
    private static partial void LogSemContexto(ILogger logger);

    [LoggerMessage(EventId = 1402, Level = LogLevel.Error,
        Message = "Falha ao registrar o consumo da execução {ExecucaoId} ({TipoErro}); registro marcado como incompleto.")]
    private static partial void LogFalhaGravacao(ILogger logger, string tipoErro, Guid execucaoId);
}
