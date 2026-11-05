using System.Reflection;
using System.Runtime.InteropServices;

namespace AuditoriaNFe.Tests.Arquitetura;

/// <summary>Dependências entre camadas (constitution 3.3, research R-28).</summary>
public sealed class DependenciasCamadasTests
{
    private const string Dominio = "AuditoriaNFe.Domain";
    private const string Aplicacao = "AuditoriaNFe.Application";
    private const string Infraestrutura = "AuditoriaNFe.Infrastructure";
    private const string Api = "Api.Agente";
    private const string Simulador = "Sofia.Simulator";

    private static readonly string[] ProjetosDoAgente = [Dominio, Aplicacao, Infraestrutura, Api];

    private static readonly string[] ProibidosNaAplicacao =
    [
        "OpenAI",
        "Microsoft.Extensions.AI.OpenAI",
        "Microsoft.Agents.AI.OpenAI",
        "ModelContextProtocol",
        "A2A",
        "Microsoft.Agents.AI.A2A",
        "Microsoft.Agents.AI.Hosting",
        "MongoDB",
        "Npgsql",
    ];

    public static TheoryData<string, string[]> ReferenciasEsperadas => new()
    {
        { Dominio, [] },
        { Aplicacao, [Dominio] },
        { Infraestrutura, [Aplicacao, Dominio] },
        { Api, [Aplicacao, Infraestrutura] },
        { Simulador, [] },
    };

    [Theory]
    [MemberData(nameof(ReferenciasEsperadas))]
    public void Referencias_entre_projetos_seguem_R28(string projeto, string[] esperadas)
    {
        var referencias = Repositorio.Projeto(projeto)
            .Descendants("ProjectReference")
            .Select(r => ((string?)r.Attribute("Include") ?? string.Empty).Replace('\\', '/'))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal);

        Assert.Equal(esperadas.Order(StringComparer.Ordinal), referencias);
    }

    [Fact]
    public void Dominio_nao_referencia_pacote_externo()
    {
        Assert.Empty(Repositorio.Projeto(Dominio).Descendants("PackageReference"));

        var diretorioRuntime = RuntimeEnvironment.GetRuntimeDirectory();
        var externas = Assembly.Load(Dominio).GetReferencedAssemblies()
            .Select(r => r.Name ?? string.Empty)
            .Where(nome => !File.Exists(Path.Combine(diretorioRuntime, $"{nome}.dll")));

        Assert.Empty(externas);
    }

    [Fact]
    public void Aplicacao_nao_referencia_OpenAI_MCP_A2A_MongoDB_nem_Npgsql()
    {
        var declarados = Repositorio.Projeto(Aplicacao)
            .Descendants("PackageReference")
            .Select(p => (string?)p.Attribute("Include") ?? string.Empty);
        var compilados = Assembly.Load(Aplicacao).GetReferencedAssemblies()
            .Select(r => r.Name ?? string.Empty);

        Assert.DoesNotContain(declarados.Concat(compilados), ProibidoNaAplicacao);
    }

    [Fact]
    public void Simulador_nao_referencia_projetos_do_agente()
    {
        var compilados = Assembly.Load(Simulador).GetReferencedAssemblies()
            .Select(r => r.Name ?? string.Empty);

        Assert.Empty(Repositorio.Projeto(Simulador).Descendants("ProjectReference"));
        Assert.DoesNotContain(compilados, nome => ProjetosDoAgente.Contains(nome, StringComparer.OrdinalIgnoreCase));
    }

    private static bool ProibidoNaAplicacao(string nome) =>
        ProibidosNaAplicacao.Any(p =>
            nome.Equals(p, StringComparison.OrdinalIgnoreCase) ||
            nome.StartsWith($"{p}.", StringComparison.OrdinalIgnoreCase));
}
