using System.Text.Json;
using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Application.Contratos;

/// <summary>
/// Envelope de resposta v1, igual nos dois canais (data-model §16; schema <c>EnvelopeV1</c> de
/// <c>contracts/api-agente.openapi.yaml</c>). Todos os campos são preenchidos pelo sistema (P09).
/// </summary>
public sealed record EnvelopeV1
{
    public const string Versao = "1.0";

    public string VersaoEnvelope => Versao;
    public string VersaoContratoIa => ContratoIa.Versao;
    public required Guid ExecucaoId { get; init; }
    public Guid? SessaoId { get; init; }
    public string? CorrelacaoId { get; init; }

    /// <summary><c>Recebida</c>, <c>EmProcessamento</c>, <c>Concluida</c>, <c>Falhou</c> ou <c>Rejeitada</c>.</summary>
    public required string Estado { get; init; }

    public required DateTimeOffset DataHora { get; init; }

    /// <summary><c>Direto</c> ou <c>A2A</c>.</summary>
    public required string Canal { get; init; }

    /// <summary><c>false</c> para o usuário "anonimo" (FR-052c).</summary>
    public required bool UsuarioIdentificado { get; init; }

    public required IReadOnlyList<EtapaV1> Etapas { get; init; }
    public ParecerV1? Parecer { get; init; }
    public RespostaPerguntaV1? Resposta { get; init; }
    public IReadOnlyList<RecomendacaoV1>? Recomendacoes { get; init; }
    public ErroV1? Erro { get; init; }
    public MetadadosV1? Metadados { get; init; }

    /// <summary>Estado, etapas, erro e metadados vindos da execução.</summary>
    public static EnvelopeV1 De(Execucao execucao, DateTimeOffset dataHora, long duracaoMs)
    {
        ArgumentNullException.ThrowIfNull(execucao);
        return new EnvelopeV1
        {
            ExecucaoId = execucao.Id,
            SessaoId = execucao.SessaoId,
            CorrelacaoId = execucao.CorrelacaoId,
            Estado = execucao.Estado.ToString(),
            DataHora = dataHora,
            Canal = execucao.Canal.ToString(),
            UsuarioIdentificado = execucao.UsuarioIdentificado,
            Etapas = execucao.Etapas.Select(EtapaV1.De).ToList(),
            Erro = execucao.Erro is null ? null : new ErroV1(execucao.Erro.Codigo, execucao.Erro.Mensagem, execucao.Erro.Arquivo),
            Metadados = new MetadadosV1(
                execucao.Modelo,
                duracaoMs,
                execucao.ConsumoResumo is null ? null : ConsumoV1.De(execucao.ConsumoResumo),
                execucao.Versoes.Count == 0 ? null : new Dictionary<string, string>(execucao.Versoes)),
        };
    }
}

/// <summary>Versão do contrato de saída do LLM (data-model §15).</summary>
public static class ContratoIa
{
    public const string Versao = "1.0";
}

/// <summary>Estado de uma das nove etapas (schema <c>Etapa</c>).</summary>
public sealed record EtapaV1(
    int Numero,
    string Nome,
    string Estado,
    string? Justificativa,
    DateTimeOffset? InicioEm,
    DateTimeOffset? FimEm)
{
    public static EtapaV1 De(EtapaExecucao etapa) =>
        new(etapa.Numero, etapa.Nome, etapa.Estado.ToString(), etapa.Justificativa, etapa.InicioEm, etapa.FimEm);
}

/// <summary>Erro com código estável e mensagem pt-BR, sem detalhes internos (schema <c>Erro</c>).</summary>
public sealed record ErroV1(
    string Codigo,
    string Mensagem,
    string? Arquivo = null,
    IReadOnlyDictionary<string, JsonElement>? Detalhes = null);

/// <summary>Corpo de erro sem envelope, quando não há execução (schema <c>ErroResposta</c>).</summary>
public sealed record ErroRespostaV1(Guid? ExecucaoId, ErroV1 Erro);

/// <summary>Metadados da execução (schema <c>EnvelopeV1.metadados</c>).</summary>
public sealed record MetadadosV1(
    string? Modelo,
    long DuracaoMs,
    ConsumoV1? Consumo,
    IReadOnlyDictionary<string, string>? Versoes);

/// <summary>Consumo da execução; valores desconhecidos ficam nulos (schema <c>Consumo</c>).</summary>
public sealed record ConsumoV1(
    long? TokensEntrada,
    long? TokensSaida,
    string? CustoEstimado,
    string? Moeda,
    string? Origem,
    bool RegistroCompleto)
{
    public static ConsumoV1 De(ConsumoResumo resumo) =>
        new(
            resumo.TokensEntrada,
            resumo.TokensSaida,
            resumo.CustoTotal?.ToString("F8", System.Globalization.CultureInfo.InvariantCulture),
            resumo.Moeda,
            resumo.Origem?.ToString(),
            resumo.RegistroCompleto);
}
