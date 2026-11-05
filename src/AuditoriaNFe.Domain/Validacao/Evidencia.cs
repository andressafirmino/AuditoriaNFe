namespace AuditoriaNFe.Domain.Validacao;

/// <summary>
/// Evidência que fundamenta um achado (data-model §8). Cada tipo exige os campos que permitem
/// conferi-la: a existência nos dados extraídos, no conjunto recuperado ou no registro de cálculos
/// é verificada pelo validador semântico da execução.
/// </summary>
public sealed class Evidencia
{
    public Evidencia(
        TipoEvidencia tipo,
        string? chaveAcesso = null,
        string? caminho = null,
        string? valor = null,
        string? regraId = null,
        int? versaoRegra = null,
        string? versaoCatalogo = null,
        string? chunkId = null,
        string? documento = null,
        string? versao = null,
        string? trecho = null,
        Guid? calculoId = null)
    {
        switch (tipo)
        {
            case TipoEvidencia.CampoXml:
                Exigir(chaveAcesso, nameof(chaveAcesso), tipo);
                Exigir(caminho, nameof(caminho), tipo);
                Exigir(valor, nameof(valor), tipo);
                break;
            case TipoEvidencia.RegraCatalogo:
                Exigir(regraId, nameof(regraId), tipo);
                Exigir(versaoCatalogo, nameof(versaoCatalogo), tipo);
                if (versaoRegra is not > 0)
                {
                    throw new ArgumentException("Evidência RegraCatalogo exige versaoRegra positiva.", nameof(versaoRegra));
                }

                break;
            case TipoEvidencia.TrechoManual:
                Exigir(chunkId, nameof(chunkId), tipo);
                Exigir(documento, nameof(documento), tipo);
                Exigir(versao, nameof(versao), tipo);
                break;
            case TipoEvidencia.Calculo:
                if (calculoId is null || calculoId == Guid.Empty)
                {
                    throw new ArgumentException("Evidência Calculo exige calculoId.", nameof(calculoId));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de evidência desconhecido.");
        }

        Tipo = tipo;
        ChaveAcesso = chaveAcesso;
        Caminho = caminho;
        Valor = valor;
        RegraId = regraId;
        VersaoRegra = versaoRegra;
        VersaoCatalogo = versaoCatalogo;
        ChunkId = chunkId;
        Documento = documento;
        Versao = versao;
        Trecho = trecho;
        CalculoId = calculoId;
    }

    public TipoEvidencia Tipo { get; }
    public string? ChaveAcesso { get; }
    public string? Caminho { get; }
    public string? Valor { get; }
    public string? RegraId { get; }
    public int? VersaoRegra { get; }
    public string? VersaoCatalogo { get; }
    public string? ChunkId { get; }
    public string? Documento { get; }
    public string? Versao { get; }
    public string? Trecho { get; }
    public Guid? CalculoId { get; }

    public static Evidencia CampoXml(string chaveAcesso, string caminho, string valor) =>
        new(TipoEvidencia.CampoXml, chaveAcesso: chaveAcesso, caminho: caminho, valor: valor);

    public static Evidencia RegraCatalogo(string regraId, int versaoRegra, string versaoCatalogo) =>
        new(TipoEvidencia.RegraCatalogo, regraId: regraId, versaoRegra: versaoRegra, versaoCatalogo: versaoCatalogo);

    public static Evidencia TrechoManual(string chunkId, string documento, string versao, string? trecho = null) =>
        new(TipoEvidencia.TrechoManual, chunkId: chunkId, documento: documento, versao: versao, trecho: trecho);

    public static Evidencia Calculo(Guid calculoId) => new(TipoEvidencia.Calculo, calculoId: calculoId);

    private static void Exigir(string? valor, string campo, TipoEvidencia tipo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"Evidência {tipo} exige {campo}.", campo);
        }
    }
}
