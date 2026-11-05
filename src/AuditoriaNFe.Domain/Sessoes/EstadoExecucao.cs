namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Estado de uma execução (data-model §2).</summary>
public enum EstadoExecucao
{
    Recebida,
    EmProcessamento,
    Concluida,
    Falhou,
    Rejeitada,
}
