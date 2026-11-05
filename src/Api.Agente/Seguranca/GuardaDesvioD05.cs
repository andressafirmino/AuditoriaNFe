using Api.Agente.Hospedagem;
using AuditoriaNFe.Application.Configuracao;

namespace Api.Agente.Seguranca;

/// <summary>
/// Desvio D-05 (FR-052b, research R-17): sem autenticação, o agente completo só executa em
/// <c>Development</c>. Fora dele, só o modo <c>SomenteAgentCard</c> é aceito (FR-052f, desvio D-08).
/// A verificação ocorre antes de montar o host, para que nada seja exposto.
/// </summary>
public static class GuardaDesvioD05
{
    public static void Verificar(IHostEnvironment ambiente, IConfiguration configuration)
    {
        var autenticacaoHabilitada = configuration.GetValue<bool>($"{SegurancaOpcoes.Secao}:{nameof(SegurancaOpcoes.AutenticacaoHabilitada)}");
        var modo = HospedagemOpcoes.LerModo(configuration);
        if (!ambiente.IsDevelopment() && !autenticacaoHabilitada && modo != ModoHospedagem.SomenteAgentCard)
        {
            throw new InvalidOperationException(
                "Autenticação desabilitada fora de Development (desvio D-05): fora do ambiente local de desenvolvimento, o agente só pode ser executado com Hospedagem:Modo = SomenteAgentCard (desvio D-08).");
        }
    }
}
