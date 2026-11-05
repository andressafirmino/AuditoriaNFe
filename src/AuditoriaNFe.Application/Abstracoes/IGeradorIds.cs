namespace AuditoriaNFe.Application.Abstracoes;

/// <summary>Identificadores gerados pelo sistema, GUID versão 7 (P09). O LLM nunca gera ids.</summary>
public interface IGeradorIds
{
    Guid Novo();
}
