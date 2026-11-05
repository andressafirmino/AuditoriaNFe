namespace AuditoriaNFe.Application.Configuracao;

/// <summary>
/// Termos de tributos que tornam uma recomendação sensível (FR-035a, P13). A lista vem do
/// responsável fiscal; não há valor padrão.
/// </summary>
public sealed class SensibilidadeOpcoes
{
    public const string Secao = "Sensibilidade";

    public List<string> TermosTributos { get; set; } = [];
}
