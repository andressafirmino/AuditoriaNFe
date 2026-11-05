using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Análises simultâneas (NFR-004, ERR-024).</summary>
public sealed class ConcorrenciaOpcoes
{
    public const string Secao = "Concorrencia";

    [Range(1, 1000)]
    public int MaxExecucoes { get; set; } = 10;
}
