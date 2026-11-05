using AuditoriaNFe.Application.Configuracao;

namespace Api.Agente.Seguranca;

/// <summary>
/// Desvio D-05 (FR-052b, research R-17): sem autenticação, o agente só executa em <c>Development</c>.
/// A verificação ocorre antes de montar o host, para que nada seja exposto.
/// </summary>
public static class GuardaDesvioD05
{
    public static void Verificar(IHostEnvironment ambiente, IConfiguration configuration)
    {
        var autenticacaoHabilitada = configuration.GetValue<bool>($"{SegurancaOpcoes.Secao}:{nameof(SegurancaOpcoes.AutenticacaoHabilitada)}");
        if (!ambiente.IsDevelopment() && !autenticacaoHabilitada)
        {
            throw new InvalidOperationException(
                "Autenticação desabilitada fora de Development (desvio D-05): o agente só pode ser executado no ambiente local de desenvolvimento.");
        }
    }
}
