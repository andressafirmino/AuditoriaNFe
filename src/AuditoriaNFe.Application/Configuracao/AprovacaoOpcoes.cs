using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Diretório dos checkpoints de aprovação (FR-035j).</summary>
public sealed class AprovacaoOpcoes
{
    public const string Secao = "Aprovacao";

    [Required(AllowEmptyStrings = false)]
    public string Diretorio { get; set; } = "./data/aprovacoes";
}
