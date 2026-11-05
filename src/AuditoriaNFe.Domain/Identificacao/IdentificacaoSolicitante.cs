using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AuditoriaNFe.Domain.Identificacao;

/// <summary>
/// Identidade informada pelo chamador, sem verificação na versão 1 (desvio D-05; data-model §0;
/// FR-052a, FR-052c).
/// </summary>
public sealed partial class IdentificacaoSolicitante
{
    public const string UsuarioAnonimo = "anonimo";
    public const string ChamadorNaoInformado = "nao-informado";
    public const int TamanhoMaximo = 128;

    // Caractere fora do formato aceito: a concatenação não é ambígua.
    private const char Separador = '|';

    private IdentificacaoSolicitante(string usuarioId, string chamadorId)
    {
        UsuarioId = usuarioId;
        ChamadorId = chamadorId;
        ChaveIsolamento = CalcularChave(chamadorId, usuarioId);
    }

    /// <summary>Dado pessoal: nunca em log.</summary>
    public string UsuarioId { get; }

    public string ChamadorId { get; }

    public bool UsuarioIdentificado => UsuarioId != UsuarioAnonimo;

    /// <summary>SHA-256 hexadecimal minúsculo de <c>chamadorId</c> + separador + <c>usuarioId</c>.</summary>
    public string ChaveIsolamento { get; }

    /// <summary>Valor ausente (nulo ou vazio) recebe o padrão; formato inválido devolve <c>false</c> (ERR-007).</summary>
    public static bool TentarCriar(
        string? usuarioId,
        string? chamadorId,
        [NotNullWhen(true)] out IdentificacaoSolicitante? identificacao)
    {
        var usuario = string.IsNullOrEmpty(usuarioId) ? UsuarioAnonimo : usuarioId;
        var chamador = string.IsNullOrEmpty(chamadorId) ? ChamadorNaoInformado : chamadorId;

        if (!FormatoValido(usuario) || !FormatoValido(chamador))
        {
            identificacao = null;
            return false;
        }

        identificacao = new IdentificacaoSolicitante(usuario, chamador);
        return true;
    }

    public static IdentificacaoSolicitante Criar(string? usuarioId, string? chamadorId) =>
        TentarCriar(usuarioId, chamadorId, out var identificacao)
            ? identificacao
            : throw new ArgumentException("Identificação do usuário ou do chamador em formato inválido.");

    public static bool FormatoValido(string valor) => Formato().IsMatch(valor);

    private static string CalcularChave(string chamadorId, string usuarioId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{chamadorId}{Separador}{usuarioId}"));
        return Convert.ToHexStringLower(bytes);
    }

    [GeneratedRegex(@"\A[A-Za-z0-9._:@-]{1,128}\z", RegexOptions.CultureInvariant)]
    private static partial Regex Formato();
}
