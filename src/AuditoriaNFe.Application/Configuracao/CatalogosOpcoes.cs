using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Arquivos versionados de catálogos (research R-15, R-16).</summary>
public sealed class CatalogosOpcoes
{
    public const string Secao = "Catalogos";

    [Required(AllowEmptyStrings = false)]
    public string Regras { get; set; } = "Catalogos/catalogo-regras.v1.json";

    [Required(AllowEmptyStrings = false)]
    public string Extracao { get; set; } = "Catalogos/catalogo-extracao.v1.json";

    [Required(AllowEmptyStrings = false)]
    public string Precos { get; set; } = "Catalogos/precos-openai.v1.json";
}
