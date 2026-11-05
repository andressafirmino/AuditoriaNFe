using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Contexto do usuário: retenção e limites de armazenamento e de envio ao modelo (FR-060, P15).</summary>
public sealed class ContextoUsuarioOpcoes
{
    public const string Secao = "ContextoUsuario";

    [Range(typeof(TimeSpan), "1.00:00:00", "3650.00:00:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan RetencaoInatividade { get; set; } = TimeSpan.FromDays(90);

    [Range(1, 10_000)]
    public int MaxCnpjs { get; set; } = 200;

    [Range(1, 10_000)]
    public int MaxAchados { get; set; } = 100;

    [Range(0, 1_000)]
    public int MaxItensNoPrompt { get; set; } = 20;
}
