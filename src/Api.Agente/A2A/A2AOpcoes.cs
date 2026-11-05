using System.ComponentModel.DataAnnotations;

namespace Api.Agente.A2A;

/// <summary>Endereço público e identificação do agente no contrato SOFIA v1.0 (FR-044a, FR-052d).</summary>
public sealed class A2AOpcoes
{
    public const string Secao = "A2A";

    /// <summary>URL base publicada no card; o endpoint A2A fica em <c>{UrlPublica}/a2a/auditoria-nfe</c>.</summary>
    [Required]
    public string UrlPublica { get; set; } = string.Empty;

    /// <summary>Igual ao cadastrado no registry da SOFIA e ao devolvido em <c>agente.idAgente</c>.</summary>
    [Required]
    public string IdAgente { get; set; } = string.Empty;

    /// <summary>Chamador atribuído às solicitações A2A enquanto vigorar o D-05.</summary>
    [Required]
    public string ChamadorPadrao { get; set; } = string.Empty;
}

public static class A2AOpcoesExtensions
{
    public static IServiceCollection AdicionarOpcoesA2A(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<A2AOpcoes>()
            .Bind(configuration.GetSection(A2AOpcoes.Secao))
            .ValidateDataAnnotations()
            .Validate(
                o => EhUrlBase(o.UrlPublica),
                "A2A:UrlPublica deve ser uma URL http ou https absoluta, sem credenciais, consulta ou fragmento.")
            .ValidateOnStart();

        return services;
    }

    private static bool EhUrlBase(string? valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.UserInfo)
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);
}
