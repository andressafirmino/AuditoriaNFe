using Api.Agente.Seguranca;

namespace Api.Agente.Endpoints;

/// <summary>
/// Grupo das rotas REST v1. Só ele recebe a política CORS <c>SofiaBrowser</c> (FR-052e); Agent Card,
/// A2A, saúde e documentação são mapeados fora dele.
/// </summary>
public static class RotasApiV1
{
    public const string Prefixo = "/api/v1";

    public static RouteGroupBuilder MapearApiV1(this IEndpointRouteBuilder rotas) =>
        rotas.MapGroup(Prefixo).RequireCors(CorsNavegadorExtensions.Politica);
}
