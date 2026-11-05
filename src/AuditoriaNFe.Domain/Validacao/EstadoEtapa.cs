namespace AuditoriaNFe.Domain.Validacao;

/// <summary>Estado de uma das nove etapas do fluxo (constitution 3.4; data-model §2).</summary>
public enum EstadoEtapa
{
    Executada,
    Falhou,
    NaoAplicavel,
    NaoExecutada,
}
