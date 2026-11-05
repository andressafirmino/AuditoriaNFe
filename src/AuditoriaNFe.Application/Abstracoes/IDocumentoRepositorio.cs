using AuditoriaNFe.Domain.Documentos;

namespace AuditoriaNFe.Application.Abstracoes;

/// <summary>
/// Base de documentos da sessão, sempre filtrada pela chave de isolamento. Guarda só os dados
/// extraídos, nunca o XML original (FR-037a).
/// </summary>
public interface IDocumentoRepositorio
{
    Task<IReadOnlyList<DocumentoNFe>> ListarPorSessaoAsync(Guid sessaoId, string chaveIsolamento, CancellationToken cancellationToken);

    Task InserirAsync(DocumentoNFe documento, CancellationToken cancellationToken);
}
