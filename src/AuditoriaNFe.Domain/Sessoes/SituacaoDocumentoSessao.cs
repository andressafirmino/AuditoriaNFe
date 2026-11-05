namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Situação de um documento em relação à base da sessão (FR-009).</summary>
public enum SituacaoDocumentoSessao
{
    /// <summary>Chave de acesso ainda não vista na sessão.</summary>
    Novo,

    /// <summary>Mesma chave e mesmo conteúdo (ERR-011).</summary>
    Duplicado,

    /// <summary>Mesma chave com conteúdo diferente (ERR-012).</summary>
    Divergente,
}
