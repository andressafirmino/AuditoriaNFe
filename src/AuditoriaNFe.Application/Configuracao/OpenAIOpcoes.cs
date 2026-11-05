namespace AuditoriaNFe.Application.Configuracao;

/// <summary>
/// Cliente OpenAI. Sem modelo padrão: a escolha vem da avaliação (DP-05). A chave vem só de variável
/// de ambiente ou de segredo, nunca do repositório.
/// </summary>
public sealed class OpenAIOpcoes
{
    public const string Secao = "OpenAI";

    public string? ApiKey { get; set; }

    public string? Modelo { get; set; }
}
