using System.Text.RegularExpressions;

namespace AuditoriaNFe.Domain.Sessoes;

/// <summary>Erro que encerrou a execução: código ERR-xxx, mensagem pt-BR e arquivo afetado (data-model §2).</summary>
public sealed partial class ErroExecucao
{
    public ErroExecucao(string codigo, string mensagem, string? arquivo = null)
    {
        if (codigo is null || !FormatoCodigo().IsMatch(codigo))
        {
            throw new ArgumentException("Código de erro deve seguir o formato ERR-000.", nameof(codigo));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(mensagem);
        Codigo = codigo;
        Mensagem = mensagem;
        Arquivo = arquivo;
    }

    public string Codigo { get; }
    public string Mensagem { get; }
    public string? Arquivo { get; }

    [GeneratedRegex(@"\AERR-[0-9]{3}\z", RegexOptions.CultureInvariant)]
    private static partial Regex FormatoCodigo();
}
