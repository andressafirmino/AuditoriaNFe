namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Tipo da solicitação (data-model §2).</summary>
public enum TipoExecucao
{
    AnaliseLote,
    Pergunta,
    AnaliseComPergunta,
    ConsultaRecomendacoes,
}
