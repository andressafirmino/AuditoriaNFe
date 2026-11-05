namespace AuditoriaNFe.Domain.Documentos;

/// <summary>Valor de um campo do catálogo de extração, com o caminho do campo no XML (data-model §4).</summary>
public sealed class CampoExtraido
{
    public CampoExtraido(string caminho, string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminho);
        ArgumentNullException.ThrowIfNull(valor);
        Caminho = caminho;
        Valor = valor;
    }

    public string Caminho { get; }
    public string Valor { get; }
}
