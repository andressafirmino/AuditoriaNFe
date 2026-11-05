using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Application.Abstracoes;

/// <summary>
/// Sessões no armazenamento (FR-036 a FR-042). Toda operação recebe a chave de isolamento: sessão de
/// outra chave é tratada como inexistente (FR-039). Indisponibilidade gera
/// <see cref="ArmazenamentoIndisponivelException"/> (ERR-025).
/// </summary>
public interface ISessaoRepositorio
{
    Task<Sessao?> ObterAsync(Guid sessaoId, string chaveIsolamento, CancellationToken cancellationToken);

    Task InserirAsync(Sessao sessao, CancellationToken cancellationToken);

    Task AtualizarAsync(Sessao sessao, CancellationToken cancellationToken);
}
