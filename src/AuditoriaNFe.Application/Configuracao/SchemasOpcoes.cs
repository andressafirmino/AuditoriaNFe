using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Pasta dos pacotes de schema e do manifesto (research R-14).</summary>
public sealed class SchemasOpcoes
{
    public const string Secao = "Schemas";

    [Required(AllowEmptyStrings = false)]
    public string Diretorio { get; set; } = "Schemas/NFe";
}
