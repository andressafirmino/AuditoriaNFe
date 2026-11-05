namespace AuditoriaNFe.Application.Configuracao;

/// <summary>
/// Autenticação (desvio D-05). Na versão 1 só <c>false</c> é aceito, e apenas em <c>Development</c>
/// (guarda de inicialização do host; research R-17).
/// </summary>
public sealed class SegurancaOpcoes
{
    public const string Secao = "Seguranca";

    public bool AutenticacaoHabilitada { get; set; }
}
