namespace AuditoriaNFe.Domain.Validacao;

/// <summary>Achado de uma execução (data-model §7; P04, FR-014, FR-020 a FR-023).</summary>
public sealed class Achado
{
    public Achado(
        Guid id,
        CategoriaAchado categoria,
        Severidade severidade,
        OrigemAchado origem,
        string descricao,
        string? arquivo = null,
        string? chaveAcesso = null,
        string? campo = null,
        int? linha = null,
        int? posicao = null,
        int? item = null,
        string? valorInformado = null,
        string? valorEsperado = null,
        string? regraId = null,
        int? versaoRegra = null,
        string? pacoteSchema = null,
        IReadOnlyList<Evidencia>? evidencias = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O achado exige id gerado pelo sistema.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        var listaEvidencias = evidencias?.ToList() ?? [];

        if (categoria == CategoriaAchado.ErroEstrutural && severidade != Severidade.Alta)
        {
            throw new ArgumentException("Erros estruturais têm sempre severidade Alta (FR-023).", nameof(severidade));
        }

        if (categoria == CategoriaAchado.InconsistenciaNegocio)
        {
            if (string.IsNullOrWhiteSpace(valorInformado) || string.IsNullOrWhiteSpace(valorEsperado))
            {
                throw new ArgumentException(
                    "Inconsistência de negócio exige valorInformado e valorEsperado (FR-014).", nameof(valorInformado));
            }

            if (string.IsNullOrWhiteSpace(regraId) || versaoRegra is not > 0)
            {
                throw new ArgumentException("Inconsistência de negócio exige regraId e versaoRegra.", nameof(regraId));
            }
        }

        if (categoria == CategoriaAchado.IndicioIa && listaEvidencias.Count == 0)
        {
            throw new ArgumentException("Indício da IA exige ao menos uma evidência (FR-021).", nameof(evidencias));
        }

        Id = id;
        Categoria = categoria;
        Severidade = severidade;
        Origem = origem;
        Descricao = descricao;
        Arquivo = arquivo;
        ChaveAcesso = chaveAcesso;
        Campo = campo;
        Linha = linha;
        Posicao = posicao;
        Item = item;
        ValorInformado = valorInformado;
        ValorEsperado = valorEsperado;
        RegraId = regraId;
        VersaoRegra = versaoRegra;
        PacoteSchema = pacoteSchema;
        Evidencias = listaEvidencias.AsReadOnly();
    }

    public Guid Id { get; }
    public CategoriaAchado Categoria { get; }
    public Severidade Severidade { get; }
    public OrigemAchado Origem { get; }
    public string Descricao { get; }
    public string? Arquivo { get; }
    public string? ChaveAcesso { get; }

    /// <summary>Caminho do campo no XML.</summary>
    public string? Campo { get; }

    public int? Linha { get; }
    public int? Posicao { get; }
    public int? Item { get; }
    public string? ValorInformado { get; }
    public string? ValorEsperado { get; }
    public string? RegraId { get; }
    public int? VersaoRegra { get; }
    public string? PacoteSchema { get; }
    public IReadOnlyList<Evidencia> Evidencias { get; }

    /// <summary>Erro de validação de schema: exige o pacote aplicado, com linha e posição (FR-011).</summary>
    public static Achado ErroDeSchema(
        Guid id,
        string descricao,
        string pacoteSchema,
        string arquivo,
        int? linha,
        int? posicao,
        string? chaveAcesso = null,
        string? campo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pacoteSchema);
        return new Achado(
            id,
            CategoriaAchado.ErroEstrutural,
            Severidade.Alta,
            OrigemAchado.Deterministica,
            descricao,
            arquivo: arquivo,
            chaveAcesso: chaveAcesso,
            campo: campo,
            linha: linha,
            posicao: posicao,
            pacoteSchema: pacoteSchema);
    }
}
