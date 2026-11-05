namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Origem dos valores de consumo (data-model §14).</summary>
public enum OrigemValoresConsumo
{
    InformadoProvedor,
    EstimadoSistema,
}

/// <summary>
/// Somatório das chamadas ao provedor de uma execução (data-model §2). Valores desconhecidos ficam
/// nulos, nunca zero por suposição; <see cref="RegistroCompleto"/> é <c>false</c> quando alguma
/// chamada não pôde ser gravada (P14).
/// </summary>
public sealed record ConsumoResumo(
    long? TokensEntrada,
    long? TokensEntradaCache,
    long? TokensSaida,
    decimal? CustoTotal,
    string? Moeda,
    OrigemValoresConsumo? Origem,
    bool RegistroCompleto);
