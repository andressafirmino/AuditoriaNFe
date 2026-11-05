using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Domain.Documentos;
using MongoDB.Driver;

namespace AuditoriaNFe.Infrastructure.Persistencia.Mongo;

/// <summary>Grava só a entidade <see cref="DocumentoNFe"/>, que não tem o XML original (FR-037a).</summary>
public sealed class DocumentoRepositorio(ContextoMongo contexto) : IDocumentoRepositorio
{
    public Task<IReadOnlyList<DocumentoNFe>> ListarPorSessaoAsync(Guid sessaoId, string chaveIsolamento, CancellationToken cancellationToken) =>
        contexto.ExecutarAsync<IReadOnlyList<DocumentoNFe>>(
            async token => await contexto.Documentos
                .Find(Builders<DocumentoNFe>.Filter.Eq(d => d.ChaveIsolamento, chaveIsolamento)
                    & Builders<DocumentoNFe>.Filter.Eq(d => d.SessaoId, sessaoId))
                .ToListAsync(token)
                .ConfigureAwait(false),
            cancellationToken);

    public Task InserirAsync(DocumentoNFe documento, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documento);
        return contexto.ExecutarAsync(token => contexto.Documentos.InsertOneAsync(documento, cancellationToken: token), cancellationToken);
    }
}
