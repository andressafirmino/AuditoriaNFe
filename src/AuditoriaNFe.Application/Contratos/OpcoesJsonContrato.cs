using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AuditoriaNFe.Application.Contratos;

/// <summary>Serialização dos contratos v1: nomes em camelCase e nulos explícitos, como no OpenAPI.</summary>
public static class OpcoesJsonContrato
{
    public static JsonSerializerOptions Padrao { get; } = Criar();

    public static void Aplicar(JsonSerializerOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoes.DictionaryKeyPolicy = null;
        opcoes.PropertyNameCaseInsensitive = true;
    }

    private static JsonSerializerOptions Criar()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        Aplicar(opcoes);
        opcoes.MakeReadOnly();
        return opcoes;
    }
}
