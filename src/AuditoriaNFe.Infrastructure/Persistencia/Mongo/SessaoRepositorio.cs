using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Domain.Sessoes;
using MongoDB.Driver;

namespace AuditoriaNFe.Infrastructure.Persistencia.Mongo;

public sealed class SessaoRepositorio(ContextoMongo contexto) : ISessaoRepositorio
{
    public Task<Sessao?> ObterAsync(Guid sessaoId, string chaveIsolamento, CancellationToken cancellationToken) =>
        contexto.ExecutarAsync<Sessao?>(
            async token => await contexto.Sessoes.Find(Filtro(sessaoId, chaveIsolamento)).FirstOrDefaultAsync(token).ConfigureAwait(false),
            cancellationToken);

    public Task InserirAsync(Sessao sessao, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sessao);
        return contexto.ExecutarAsync(token => contexto.Sessoes.InsertOneAsync(sessao, cancellationToken: token), cancellationToken);
    }

    public Task AtualizarAsync(Sessao sessao, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sessao);
        return contexto.ExecutarAsync(
            async token =>
            {
                var resultado = await contexto.Sessoes
                    .ReplaceOneAsync(Filtro(sessao.Id, sessao.ChaveIsolamento), sessao, cancellationToken: token)
                    .ConfigureAwait(false);
                if (resultado.MatchedCount == 0)
                {
                    throw new InvalidOperationException("Sessão não encontrada para a chave de isolamento.");
                }
            },
            cancellationToken);
    }

    private static FilterDefinition<Sessao> Filtro(Guid sessaoId, string chaveIsolamento) =>
        Builders<Sessao>.Filter.Eq(s => s.Id, sessaoId) & Builders<Sessao>.Filter.Eq(s => s.ChaveIsolamento, chaveIsolamento);
}
