using System.Text.RegularExpressions;

namespace AuditoriaNFe.Domain.Documentos;

/// <summary>
/// Formato da chave de acesso: tipo <c>TChNFe</c> de <c>tiposBasico_v4.00.xsd</c> do PL_010e_v1.02,
/// com o CNPJ alfanumérico nas posições 7 a 18 (NT 2026.004). Aceita também as chaves numéricas dos
/// pacotes anteriores.
/// </summary>
public static partial class ChaveAcessoNFe
{
    public const int Tamanho = 44;

    public static bool FormatoValido(string? chave) => chave is not null && Formato().IsMatch(chave);

    [GeneratedRegex(@"\A[0-9]{6}[0-9A-Z]{12}[0-9]{26}\z", RegexOptions.CultureInvariant)]
    private static partial Regex Formato();
}
