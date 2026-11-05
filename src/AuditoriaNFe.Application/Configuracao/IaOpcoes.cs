using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Provedor de IA usado na etapa 7.</summary>
public enum ProvedorIa
{
    OpenAI,

    /// <summary>Respostas simuladas para uso local; recusado fora de <c>Development</c>.</summary>
    Simulado,
}

/// <summary>Provedor, retentativas e timeout das chamadas de IA (P08, ERR-015; constitution 5.4).</summary>
public sealed class IaOpcoes
{
    public const string Secao = "Ia";

    [Required]
    public ProvedorIa? Provedor { get; set; }

    [Range(1, 10)]
    public int MaxTentativas { get; set; } = 3;

    [Range(typeof(TimeSpan), "00:00:01", "00:10:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan TimeoutChamada { get; set; } = TimeSpan.FromSeconds(30);
}
