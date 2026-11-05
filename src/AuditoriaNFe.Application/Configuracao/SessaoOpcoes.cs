using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Expiração das sessões (spec, Assumptions; FR-040).</summary>
public sealed class SessaoOpcoes
{
    public const string Secao = "Sessao";

    [Range(typeof(TimeSpan), "00:01:00", "30.00:00:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan ExpiracaoInatividade { get; set; } = TimeSpan.FromHours(24);
}
