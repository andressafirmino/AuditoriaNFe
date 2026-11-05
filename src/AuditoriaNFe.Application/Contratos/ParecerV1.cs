namespace AuditoriaNFe.Application.Contratos;

/// <summary>Parecer de uma análise (data-model §10; schema <c>Parecer</c>).</summary>
public sealed record ParecerV1(
    ResumoQuantitativoV1 ResumoQuantitativo,
    string ResultadoGeral,
    IReadOnlyList<DocumentoParecerV1> Documentos,
    IReadOnlyList<AchadoV1> Achados,
    IReadOnlyList<string> Limitacoes,
    IReadOnlyList<ValidacaoNaoRealizadaV1> ValidacoesNaoRealizadas,
    RevisaoHumanaV1 RevisaoHumana,
    IReadOnlyList<RecomendacaoV1> Recomendacoes,
    IntegracaoMcpV1 IntegracaoMcp,
    string ResumoTextual,
    VersoesParecerV1? Versoes);

public sealed record ResumoQuantitativoV1(int Recebidos, int Analisados, int ComAchados, int NaoAnalisados);

/// <summary>Situação de cada arquivo no parecer (FR-010d, FR-010f; schema <c>DocumentoParecer</c>).</summary>
public sealed record DocumentoParecerV1(
    string Arquivo,
    string? ChaveAcesso,
    DateOnly? DataReferencia,
    string? OrigemDataReferencia,
    string? PacoteAplicado,
    IReadOnlyList<string> PacotesTentados,
    bool EmConvivencia,
    string ValidacaoLayout,
    string ValidacaoConteudo,
    string? DuplicadoDe);

/// <summary><c>Validacao</c> é <c>Layout</c> ou <c>Conteudo</c>.</summary>
public sealed record ValidacaoNaoRealizadaV1(string Arquivo, string Validacao, string Motivo);

public sealed record RevisaoHumanaV1(bool Necessaria, IReadOnlyList<string> Motivos);

public sealed record IntegracaoMcpV1(bool Habilitada);

public sealed record VersoesParecerV1(string? ManifestoSchemas, string? CatalogoRegras);

/// <summary>Achado (data-model §7; schema <c>Achado</c>).</summary>
public sealed record AchadoV1(
    Guid Id,
    string Categoria,
    string Severidade,
    string Origem,
    string Descricao,
    string? Arquivo,
    string? ChaveAcesso,
    string? Campo,
    int? Linha,
    int? Posicao,
    int? Item,
    string? ValorInformado,
    string? ValorEsperado,
    string? RegraId,
    int? VersaoRegra,
    string? PacoteSchema,
    IReadOnlyList<EvidenciaV1> Evidencias);

/// <summary>Evidência (data-model §8; schema <c>Evidencia</c>).</summary>
public sealed record EvidenciaV1(
    string Tipo,
    string? ChaveAcesso,
    string? Caminho,
    string? Valor,
    string? RegraId,
    int? VersaoRegra,
    string? VersaoCatalogo,
    string? ChunkId,
    string? Documento,
    string? Versao,
    string? Trecho,
    Guid? CalculoId);

/// <summary>Recomendação na visão por estado: texto só em <c>Liberada</c> e <c>Aprovada</c> (FR-035c, FR-035g).</summary>
public sealed record RecomendacaoV1(
    Guid Id,
    string Estado,
    string? Aviso,
    string? Texto,
    IReadOnlyList<Guid> AchadosVinculados)
{
    public const string RotuloFixo = "Recomendação demonstrativa";

    public string Rotulo => RotuloFixo;
}
