using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Domain.Sessoes;
using MongoDB.Driver;

namespace AuditoriaNFe.Infrastructure.Persistencia.Mongo;

public sealed class ExecucaoRepositorio(ContextoMongo contexto) : IExecucaoRepositorio
{
    public Task<Execucao?> ObterAsync(Guid execucaoId, string chaveIsolamento, CancellationToken cancellationToken) =>
        contexto.ExecutarAsync<Execucao?>(
            async token => await contexto.Execucoes.Find(Filtro(execucaoId, chaveIsolamento)).FirstOrDefaultAsync(token).ConfigureAwait(false),
            cancellationToken);

    public Task<IReadOnlyList<Execucao>> ListarPorSessaoAsync(Guid sessaoId, string chaveIsolamento, CancellationToken cancellationToken) =>
        contexto.ExecutarAsync<IReadOnlyList<Execucao>>(
            async token => await contexto.Execucoes
                .Find(Builders<Execucao>.Filter.Eq(e => e.ChaveIsolamento, chaveIsolamento)
                    & Builders<Execucao>.Filter.Eq(e => e.SessaoId, sessaoId))
                .SortByDescending(e => e.IniciadaEm)
                .ToListAsync(token)
                .ConfigureAwait(false),
            cancellationToken);

    public Task InserirAsync(Execucao execucao, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(execucao);
        return contexto.ExecutarAsync(token => contexto.Execucoes.InsertOneAsync(execucao, cancellationToken: token), cancellationToken);
    }

    public Task AtualizarAsync(Execucao execucao, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(execucao);
        return contexto.ExecutarAsync(
            async token =>
            {
                var resultado = await contexto.Execucoes
                    .ReplaceOneAsync(Filtro(execucao.Id, execucao.ChaveIsolamento), execucao, cancellationToken: token)
                    .ConfigureAwait(false);
                if (resultado.MatchedCount == 0)
                {
                    throw new InvalidOperationException("Execução não encontrada para a chave de isolamento.");
                }
            },
            cancellationToken);
    }

    private static FilterDefinition<Execucao> Filtro(Guid execucaoId, string chaveIsolamento) =>
        Builders<Execucao>.Filter.Eq(e => e.Id, execucaoId) & Builders<Execucao>.Filter.Eq(e => e.ChaveIsolamento, chaveIsolamento);
}
