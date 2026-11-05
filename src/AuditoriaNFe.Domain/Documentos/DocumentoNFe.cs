using System.Text.RegularExpressions;

namespace AuditoriaNFe.Domain.Documentos;

/// <summary>
/// Documento identificado e seus dados extraídos (data-model §4). Não guarda o XML original
/// (FR-037a): só os campos do catálogo de extração.
/// </summary>
public sealed partial class DocumentoNFe
{
    public DocumentoNFe(
        Guid id,
        Guid sessaoId,
        string chaveIsolamento,
        string chaveAcesso,
        string modelo,
        string versaoLayout,
        bool possuiProtocolo,
        string dhEmi,
        string? dhRecbto,
        DateOnly dataReferencia,
        OrigemDataReferencia origemDataReferencia,
        string pacoteSchema,
        string hashConteudo,
        IEnumerable<CampoExtraido> dadosExtraidos,
        string arquivoOrigem,
        Guid execucaoOrigem)
    {
        if (id == Guid.Empty || sessaoId == Guid.Empty || execucaoOrigem == Guid.Empty)
        {
            throw new ArgumentException("Documento exige id, sessão e execução de origem.");
        }

        if (chaveIsolamento is null || !Sha256Hex().IsMatch(chaveIsolamento))
        {
            throw new ArgumentException("Chave de isolamento inválida.", nameof(chaveIsolamento));
        }

        if (!ChaveAcessoNFe.FormatoValido(chaveAcesso))
        {
            throw new ArgumentException("Chave de acesso fora do formato TChNFe.", nameof(chaveAcesso));
        }

        if (hashConteudo is null || !Sha256Hex().IsMatch(hashConteudo))
        {
            throw new ArgumentException("Hash deve ser SHA-256 hexadecimal minúsculo.", nameof(hashConteudo));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(modelo);
        ArgumentException.ThrowIfNullOrWhiteSpace(versaoLayout);
        ArgumentException.ThrowIfNullOrWhiteSpace(dhEmi);
        ArgumentException.ThrowIfNullOrWhiteSpace(pacoteSchema);
        ArgumentException.ThrowIfNullOrWhiteSpace(arquivoOrigem);
        ArgumentNullException.ThrowIfNull(dadosExtraidos);

        Id = id;
        SessaoId = sessaoId;
        ChaveIsolamento = chaveIsolamento;
        ChaveAcesso = chaveAcesso!;
        Modelo = modelo;
        VersaoLayout = versaoLayout;
        PossuiProtocolo = possuiProtocolo;
        DhEmi = dhEmi;
        DhRecbto = dhRecbto;
        DataReferencia = dataReferencia;
        OrigemDataReferencia = origemDataReferencia;
        PacoteSchema = pacoteSchema;
        HashConteudo = hashConteudo;
        DadosExtraidos = dadosExtraidos.ToList().AsReadOnly();
        ArquivoOrigem = arquivoOrigem;
        ExecucaoOrigem = execucaoOrigem;
    }

    public Guid Id { get; }
    public Guid SessaoId { get; }
    public string ChaveIsolamento { get; }
    public string ChaveAcesso { get; }
    public string Modelo { get; }
    public string VersaoLayout { get; }
    public bool PossuiProtocolo { get; }

    /// <summary>Como no XML, com o deslocamento de fuso.</summary>
    public string DhEmi { get; }

    /// <summary>Como no XML, quando houver protocolo.</summary>
    public string? DhRecbto { get; }

    public DateOnly DataReferencia { get; }
    public OrigemDataReferencia OrigemDataReferencia { get; }
    public string PacoteSchema { get; }
    public string HashConteudo { get; }
    public IReadOnlyList<CampoExtraido> DadosExtraidos { get; }
    public string ArquivoOrigem { get; }
    public Guid ExecucaoOrigem { get; }

    [GeneratedRegex(@"\A[0-9a-f]{64}\z", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Hex();
}
