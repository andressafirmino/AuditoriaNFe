using AuditoriaNFe.Application.Configuracao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuditoriaNFe.Tests.Aplicacao.Configuracao;

public sealed class OpcoesTests
{
    private static readonly Dictionary<string, string?> Minima = new()
    {
        ["Ia:Provedor"] = "Simulado",
        ["Persistencia:TimeoutMongo"] = "00:00:05",
        ["Persistencia:TimeoutPostgres"] = "00:00:05",
    };

    private static void Validar(IDictionary<string, string?> valores)
    {
        var configuracao = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
        using var provedor = new ServiceCollection().AdicionarOpcoesAuditoria(configuracao).BuildServiceProvider();
        provedor.GetRequiredService<IStartupValidator>().Validate();
    }

    private static Dictionary<string, string?> Com(params (string Chave, string? Valor)[] ajustes)
    {
        var valores = new Dictionary<string, string?>(Minima);
        foreach (var (chave, valor) in ajustes)
        {
            valores[chave] = valor;
        }

        return valores;
    }

    [Fact]
    public void Configuracao_minima_e_valida_e_usa_os_valores_iniciais_do_plano()
    {
        var configuracao = new ConfigurationBuilder().AddInMemoryCollection(Minima).Build();
        using var provedor = new ServiceCollection().AdicionarOpcoesAuditoria(configuracao).BuildServiceProvider();

        provedor.GetRequiredService<IStartupValidator>().Validate();

        Assert.Equal(20, provedor.GetRequiredService<IOptions<LimitesOpcoes>>().Value.QuantidadeArquivos);
        Assert.Equal(TimeSpan.FromHours(24), provedor.GetRequiredService<IOptions<SessaoOpcoes>>().Value.ExpiracaoInatividade);
        Assert.False(provedor.GetRequiredService<IOptions<McpOpcoes>>().Value.Habilitado);
        Assert.Null(provedor.GetRequiredService<IOptions<OpenAIOpcoes>>().Value.Modelo);
    }

    [Theory]
    [InlineData("Persistencia:TimeoutMongo")]
    [InlineData("Persistencia:TimeoutPostgres")]
    [InlineData("Ia:Provedor")]
    public void Chaves_obrigatorias_ausentes_impedem_a_inicializacao(string chave)
    {
        var valores = Com((chave, null));
        valores.Remove(chave);

        Assert.Throws<OptionsValidationException>(() => Validar(valores));
    }

    [Theory]
    [InlineData("Seguranca:AutenticacaoHabilitada", "true")]
    [InlineData("Limites:CorpoHttpBytes", "1024")]
    [InlineData("Mcp:Habilitado", "true")]
    [InlineData("Persistencia:TimeoutMongo", "00:05:00")]
    [InlineData("Limites:QuantidadeArquivos", "0")]
    [InlineData("LimiteTaxa:Janela", "00:00:00")]
    public void Valores_invalidos_impedem_a_inicializacao(string chave, string valor)
    {
        Assert.Throws<OptionsValidationException>(() => Validar(Com((chave, valor))));
    }

    [Fact]
    public void OpenAI_exige_chave_e_modelo_sem_padrao()
    {
        Assert.Throws<OptionsValidationException>(() => Validar(Com(("Ia:Provedor", "OpenAI"))));
        Assert.Throws<OptionsValidationException>(() =>
            Validar(Com(("Ia:Provedor", "OpenAI"), ("OpenAI:ApiKey", "valor-de-teste"))));

        Validar(Com(("Ia:Provedor", "OpenAI"), ("OpenAI:ApiKey", "valor-de-teste"), ("OpenAI:Modelo", "gpt-4.1-mini")));
    }

    [Fact]
    public void Mcp_habilitado_com_url_absoluta_e_aceito()
    {
        Validar(Com(("Mcp:Habilitado", "true"), ("Mcp:Endpoint", "http://localhost:5320/mcp")));
    }
}
