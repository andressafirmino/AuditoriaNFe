using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AuditoriaNFe.Infrastructure.Ia;

/// <summary>Preço por milhão de tokens de um modelo (DP-22).</summary>
public sealed record PrecoModelo(decimal Entrada, decimal? EntradaCache, decimal Saida);

/// <summary>
/// Tabela versionada <c>Catalogos/precos-openai.v{n}.json</c> (research R-16). O modelo é encontrado
/// pelo nome exato ou pelo nome seguido do sufixo de data que a OpenAI devolve (por exemplo,
/// <c>gpt-4.1-mini-2025-04-14</c>).
/// </summary>
public sealed partial class TabelaPrecos
{
    private readonly IReadOnlyDictionary<string, PrecoModelo> _modelos;

    public TabelaPrecos(string versao, string moeda, IReadOnlyDictionary<string, PrecoModelo> modelos)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versao);
        ArgumentException.ThrowIfNullOrWhiteSpace(moeda);
        ArgumentNullException.ThrowIfNull(modelos);
        if (modelos.Values.Any(p => p.Entrada < 0 || p.Saida < 0 || p.EntradaCache < 0))
        {
            throw new ArgumentException("Preços não podem ser negativos.", nameof(modelos));
        }

        Versao = versao;
        Moeda = moeda;
        _modelos = new Dictionary<string, PrecoModelo>(modelos, StringComparer.Ordinal);
    }

    public string Versao { get; }
    public string Moeda { get; }

    public static TabelaPrecos Carregar(string caminho)
    {
        using var arquivo = File.OpenRead(caminho);
        var conteudo = JsonSerializer.Deserialize<ArquivoPrecos>(arquivo, OpcoesLeitura)
            ?? throw new InvalidDataException($"Tabela de preços vazia: {Path.GetFileName(caminho)}.");

        return new TabelaPrecos(
            conteudo.Versao ?? throw new InvalidDataException("Tabela de preços sem versão."),
            conteudo.Moeda ?? throw new InvalidDataException("Tabela de preços sem moeda."),
            (conteudo.Modelos ?? []).ToDictionary(
                m => m.Key,
                m => new PrecoModelo(m.Value.Entrada, m.Value.EntradaCache, m.Value.Saida),
                StringComparer.Ordinal));
    }

    public bool TentarObter(string? modelo, out PrecoModelo? preco)
    {
        preco = null;
        if (string.IsNullOrWhiteSpace(modelo))
        {
            return false;
        }

        if (_modelos.TryGetValue(modelo, out preco))
        {
            return true;
        }

        var sufixo = SufixoData().Match(modelo);
        return sufixo.Success && _modelos.TryGetValue(modelo[..sufixo.Index], out preco);
    }

    private static readonly JsonSerializerOptions OpcoesLeitura = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    [GeneratedRegex(@"-[0-9]{4}-[0-9]{2}-[0-9]{2}\z", RegexOptions.CultureInvariant)]
    private static partial Regex SufixoData();

    private sealed class ArquivoPrecos
    {
        public string? Versao { get; set; }
        public string? Moeda { get; set; }
        public Dictionary<string, ArquivoPreco>? Modelos { get; set; }
    }

    private sealed class ArquivoPreco
    {
        [JsonRequired]
        public decimal Entrada { get; set; }

        public decimal? EntradaCache { get; set; }

        [JsonRequired]
        public decimal Saida { get; set; }
    }
}
