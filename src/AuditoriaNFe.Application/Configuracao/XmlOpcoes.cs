using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Limites do leitor XML seguro (constitution 5.2).</summary>
public sealed class XmlOpcoes
{
    public const string Secao = "Xml";

    [Range(1, 1000)]
    public int ProfundidadeMaxima { get; set; } = 64;

    /// <summary>Sem valor, deriva de <see cref="LimitesOpcoes.TamanhoTotalBytes"/>.</summary>
    [Range(1, long.MaxValue)]
    public long? MaxCaracteres { get; set; }

    public long MaxCaracteresEfetivo(LimitesOpcoes limites) => MaxCaracteres ?? limites.TamanhoTotalBytes;
}
