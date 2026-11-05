using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Limitação de taxa em janela fixa por chave de isolamento (constitution 5.2, ERR-024).</summary>
public sealed class LimiteTaxaOpcoes
{
    public const string Secao = "LimiteTaxa";

    [Range(1, 100_000)]
    public int PermissoesPorJanela { get; set; } = 30;

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan Janela { get; set; } = TimeSpan.FromMinutes(1);
}
