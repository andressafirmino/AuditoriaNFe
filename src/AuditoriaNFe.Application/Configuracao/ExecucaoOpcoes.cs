using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Tempo máximo de uma execução (NFR-001, ERR-016).</summary>
public sealed class ExecucaoOpcoes
{
    public const string Secao = "Execucao";

    [Range(typeof(TimeSpan), "00:00:05", "00:30:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan TempoMaximo { get; set; } = TimeSpan.FromSeconds(120);
}
