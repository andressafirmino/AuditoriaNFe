namespace AuditoriaNFe.Infrastructure.Ia;

/// <summary>Custo de uma chamada; componentes desconhecidos ficam nulos, nunca zero.</summary>
public sealed record CustoCalculado(decimal? Entrada, decimal? Saida, decimal? Total, string? Moeda, string VersaoTabela);

/// <summary>
/// Custo estimado de uma chamada: tokens × preço ÷ 1.000.000, em 8 casas com
/// <see cref="MidpointRounding.AwayFromZero"/> (research R-16). Sem preço do modelo ou sem tokens, o
/// custo correspondente é NULL; o total só existe quando entrada e saída são conhecidas.
/// </summary>
public sealed class CalculadoraCusto(TabelaPrecos tabela)
{
    private const decimal Milhao = 1_000_000m;
    private const int Casas = 8;

    public string VersaoTabela => tabela.Versao;

    /// <param name="tokensEntrada">Total de tokens de entrada, incluindo os em cache, como informa o provedor.</param>
    public CustoCalculado Calcular(string? modelo, int? tokensEntrada, int? tokensEntradaCache, int? tokensSaida)
    {
        if (!tabela.TentarObter(modelo, out var preco) || preco is null)
        {
            return new CustoCalculado(null, null, null, null, tabela.Versao);
        }

        decimal? entrada = null;
        if (tokensEntrada is >= 0)
        {
            var emCache = Math.Clamp(tokensEntradaCache ?? 0, 0, tokensEntrada.Value);
            var semCache = tokensEntrada.Value - emCache;
            entrada = Arredondar(semCache * preco.Entrada / Milhao) + Arredondar(emCache * (preco.EntradaCache ?? preco.Entrada) / Milhao);
        }

        decimal? saida = tokensSaida is >= 0 ? Arredondar(tokensSaida.Value * preco.Saida / Milhao) : null;
        decimal? total = entrada is null || saida is null ? null : entrada + saida;
        var moeda = entrada is null && saida is null ? null : tabela.Moeda;

        return new CustoCalculado(entrada, saida, total, moeda, tabela.Versao);
    }

    private static decimal Arredondar(decimal valor) => Math.Round(valor, Casas, MidpointRounding.AwayFromZero);
}
