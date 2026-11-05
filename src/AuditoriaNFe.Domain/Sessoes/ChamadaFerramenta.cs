using System.Text.RegularExpressions;

namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Resultado de uma chamada de ferramenta pela IA (data-model §2).</summary>
public enum StatusChamadaFerramenta
{
    Sucesso,
    Falha,
    Timeout,
    NaoAutorizada,
}

/// <summary>
/// Registro de uma chamada de ferramenta da IA (NFR-008). Só identificadores: nome e erro aceitam
/// apenas letras, dígitos, "_" e "-", de modo que argumentos de texto livre e valores de documentos
/// não têm onde ser gravados (P16).
/// </summary>
public sealed partial class ChamadaFerramenta
{
    public ChamadaFerramenta(
        string nome,
        DateTimeOffset iniciadaEm,
        long duracaoMs,
        StatusChamadaFerramenta status,
        string? erro = null,
        Guid? calculoId = null)
    {
        if (nome is null || !Identificador().IsMatch(nome))
        {
            throw new ArgumentException("Nome de ferramenta inválido.", nameof(nome));
        }

        if (erro is not null && !Identificador().IsMatch(erro))
        {
            throw new ArgumentException("O erro da ferramenta deve ser um código, sem texto livre.", nameof(erro));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(duracaoMs);
        Nome = nome;
        IniciadaEm = iniciadaEm;
        DuracaoMs = duracaoMs;
        Status = status;
        Erro = erro;
        CalculoId = calculoId;
    }

    public string Nome { get; }
    public DateTimeOffset IniciadaEm { get; }
    public long DuracaoMs { get; }
    public StatusChamadaFerramenta Status { get; }
    public string? Erro { get; }
    public Guid? CalculoId { get; }

    [GeneratedRegex(@"\A[A-Za-z0-9_-]{1,64}\z", RegexOptions.CultureInvariant)]
    private static partial Regex Identificador();
}
