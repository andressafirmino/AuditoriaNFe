namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Trecho de manual recuperado na etapa 5 (NFR-008; data-model §2).</summary>
public sealed class TrechoConsultado
{
    public TrechoConsultado(string chunkId, string documento, string versao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chunkId);
        ArgumentException.ThrowIfNullOrWhiteSpace(documento);
        ArgumentException.ThrowIfNullOrWhiteSpace(versao);
        ChunkId = chunkId;
        Documento = documento;
        Versao = versao;
    }

    public string ChunkId { get; }
    public string Documento { get; }
    public string Versao { get; }
}
