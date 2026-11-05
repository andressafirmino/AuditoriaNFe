namespace AuditoriaNFe.Infrastructure.Persistencia.Postgres;

/// <summary>
/// Uma chamada ao provedor de IA, inclusive retentativas e cada volta do loop de ferramentas
/// (data-model §14; constitution 4.5, P14). Sem conteúdo de mensagens.
/// </summary>
public sealed class ChamadaProvedor
{
    public Guid Id { get; set; }
    public Guid ExecucaoId { get; set; }
    public string? CorrelacaoId { get; set; }
    public string Servico { get; set; } = "Api.Agente";
    public string Operacao { get; set; } = string.Empty;

    /// <summary><c>Chat</c> ou <c>Embedding</c>.</summary>
    public string TipoChamada { get; set; } = string.Empty;

    public string Provedor { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;

    /// <summary>Tentativa de geração da saída estruturada (≥ 1).</summary>
    public int Tentativa { get; set; } = 1;

    /// <summary>Tokens: NULL quando o provedor não informar; nunca zero por suposição.</summary>
    public int? TokensEntrada { get; set; }

    public int? TokensEntradaCache { get; set; }
    public int? TokensSaida { get; set; }
    public int? TokensEmbedding { get; set; }
    public DateTimeOffset IniciadaEm { get; set; }
    public int DuracaoMs { get; set; }

    /// <summary><c>Sucesso</c>, <c>Falha</c>, <c>Timeout</c> ou <c>RespostaInvalida</c>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Custos: NULL sem preço na tabela ou sem tokens.</summary>
    public decimal? CustoEntrada { get; set; }

    public decimal? CustoSaida { get; set; }
    public decimal? CustoTotal { get; set; }
    public string? Moeda { get; set; }
    public string? VersaoTabelaPrecos { get; set; }

    /// <summary><c>InformadoProvedor</c> ou <c>EstimadoSistema</c>.</summary>
    public string OrigemValores { get; set; } = string.Empty;

    /// <summary>Hash da identificação; nunca o <c>usuarioId</c> em claro.</summary>
    public string ChaveIsolamento { get; set; } = string.Empty;
}
