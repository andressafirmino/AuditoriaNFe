using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Timeout das ferramentas chamadas pela IA (constitution 4.6).</summary>
public sealed class FerramentasOpcoes
{
    public const string Secao = "Ferramentas";

    [Range(typeof(TimeSpan), "00:00:00.100", "00:01:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}
