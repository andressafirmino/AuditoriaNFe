using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Application.Consumo;

/// <summary>Valores de uma chamada ao provedor que entram no resumo da execução.</summary>
public sealed record ConsumoChamada(
    long? TokensEntrada,
    long? TokensEntradaCache,
    long? TokensSaida,
    decimal? CustoTotal,
    string? Moeda,
    OrigemValoresConsumo Origem);

/// <summary>
/// Contexto de consumo da execução corrente, propagado por <see cref="AsyncLocal{T}"/> até o
/// registro de cada chamada ao provedor (research R-09, R-16). Acumula o resumo da execução e indica
/// registro incompleto quando alguma chamada não pôde ser gravada (P14).
/// </summary>
public sealed class EscopoConsumo : IDisposable
{
    private static readonly AsyncLocal<EscopoConsumo?> EscopoAtual = new();

    private readonly EscopoConsumo? _anterior;
    private readonly Lock _trava = new();
    private readonly List<ConsumoChamada> _chamadas = [];
    private bool _registroIncompleto;
    private bool _encerrado;

    private EscopoConsumo(Guid execucaoId, string? correlacaoId, string chaveIsolamento, string operacao)
    {
        ExecucaoId = execucaoId;
        CorrelacaoId = correlacaoId;
        ChaveIsolamento = chaveIsolamento;
        Operacao = operacao;
        _anterior = EscopoAtual.Value;
    }

    public static EscopoConsumo? Atual => EscopoAtual.Value;

    public Guid ExecucaoId { get; }
    public string? CorrelacaoId { get; }
    public string ChaveIsolamento { get; }
    public string Operacao { get; }

    /// <summary>Tentativa de geração da saída estruturada; começa em 1.</summary>
    public int Tentativa { get; private set; } = 1;

    public static EscopoConsumo Iniciar(Guid execucaoId, string? correlacaoId, string chaveIsolamento, string operacao)
    {
        if (execucaoId == Guid.Empty)
        {
            throw new ArgumentException("A execução é obrigatória.", nameof(execucaoId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(chaveIsolamento);
        ArgumentException.ThrowIfNullOrWhiteSpace(operacao);

        var escopo = new EscopoConsumo(execucaoId, correlacaoId, chaveIsolamento, operacao);
        EscopoAtual.Value = escopo;
        return escopo;
    }

    public void DefinirTentativa(int tentativa)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(tentativa, 1);
        Tentativa = tentativa;
    }

    public void RegistrarChamada(ConsumoChamada chamada)
    {
        ArgumentNullException.ThrowIfNull(chamada);
        lock (_trava)
        {
            _chamadas.Add(chamada);
        }
    }

    public void RegistrarFalhaGravacao()
    {
        lock (_trava)
        {
            _registroIncompleto = true;
        }
    }

    /// <summary>
    /// Soma das chamadas: cada total só existe quando todas as chamadas informaram o valor; sem
    /// chamadas, não há valores. A origem é <c>EstimadoSistema</c> se alguma chamada for estimada.
    /// </summary>
    public ConsumoResumo Resumir()
    {
        lock (_trava)
        {
            if (_chamadas.Count == 0)
            {
                return new ConsumoResumo(null, null, null, null, null, null, !_registroIncompleto);
            }

            var custo = Somar(_chamadas.Select(c => c.CustoTotal));
            var moedas = _chamadas.Select(c => c.Moeda).Distinct().ToList();

            return new ConsumoResumo(
                Somar(_chamadas.Select(c => c.TokensEntrada)),
                Somar(_chamadas.Select(c => c.TokensEntradaCache)),
                Somar(_chamadas.Select(c => c.TokensSaida)),
                moedas.Count == 1 ? custo : null,
                custo is not null && moedas.Count == 1 ? moedas[0] : null,
                _chamadas.Any(c => c.Origem == OrigemValoresConsumo.EstimadoSistema)
                    ? OrigemValoresConsumo.EstimadoSistema
                    : OrigemValoresConsumo.InformadoProvedor,
                !_registroIncompleto);
        }
    }

    public void Dispose()
    {
        if (_encerrado)
        {
            return;
        }

        _encerrado = true;
        if (ReferenceEquals(EscopoAtual.Value, this))
        {
            EscopoAtual.Value = _anterior;
        }
    }

    private static long? Somar(IEnumerable<long?> valores)
    {
        long total = 0;
        foreach (var valor in valores)
        {
            if (valor is null)
            {
                return null;
            }

            total += valor.Value;
        }

        return total;
    }

    private static decimal? Somar(IEnumerable<decimal?> valores)
    {
        decimal total = 0;
        foreach (var valor in valores)
        {
            if (valor is null)
            {
                return null;
            }

            total += valor.Value;
        }

        return total;
    }
}
