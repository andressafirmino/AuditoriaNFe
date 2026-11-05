using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Limites da solicitação (FR-003, FR-004; research R-29).</summary>
public sealed class LimitesOpcoes
{
    public const string Secao = "Limites";

    [Range(1, long.MaxValue)]
    public long TamanhoTotalBytes { get; set; } = 5 * 1024 * 1024;

    [Range(1, 1000)]
    public int QuantidadeArquivos { get; set; } = 20;

    /// <summary>Limite do corpo HTTP; não pode ser menor que o tamanho total (multipart tem sobrecarga).</summary>
    [Range(1, long.MaxValue)]
    public long CorpoHttpBytes { get; set; } = 2 * 5 * 1024 * 1024;
}
