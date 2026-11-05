using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Histórico da sessão enviado ao agente (FR-042).</summary>
public sealed class HistoricoOpcoes
{
    public const string Secao = "Historico";

    [Range(1, 1000)]
    public int MaxMensagens { get; set; } = 40;
}
