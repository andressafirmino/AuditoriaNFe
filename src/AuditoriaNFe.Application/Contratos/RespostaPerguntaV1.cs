namespace AuditoriaNFe.Application.Contratos;

/// <summary>Resposta a uma pergunta, validada pelo sistema (schema <c>RespostaPergunta</c>).</summary>
public sealed record RespostaPerguntaV1(
    string Texto,
    IReadOnlyList<ValorCitadoV1> ValoresCitados,
    IReadOnlyList<DocumentoExcluidoV1> DocumentosExcluidos,
    bool? InformacaoIndisponivel,
    bool? ForaDeEscopo);

/// <summary>Valor citado com <c>calculoId</c> ou com chave e caminho (FR-027).</summary>
public sealed record ValorCitadoV1(string Valor, Guid? CalculoId, string? ChaveAcesso, string? Caminho);

public sealed record DocumentoExcluidoV1(string ChaveAcesso, string Motivo);
