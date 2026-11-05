using AuditoriaNFe.Infrastructure.Ia;

namespace AuditoriaNFe.Tests.Aplicacao.Consumo;

public sealed class CalculadoraCustoTests
{
    private static readonly TabelaPrecos Tabela = new(
        "precos-teste.v1",
        "USD",
        new Dictionary<string, PrecoModelo>
        {
            ["modelo-a"] = new(0.40m, 0.10m, 1.60m),
            ["modelo-sem-cache"] = new(2.00m, null, 8.00m),
            ["modelo-meio"] = new(0.025m, null, 0.025m),
        });

    private static readonly CalculadoraCusto Calculadora = new(Tabela);

    [Fact]
    public void Calcula_entrada_com_cache_saida_e_total()
    {
        var custo = Calculadora.Calcular("modelo-a", tokensEntrada: 1000, tokensEntradaCache: 200, tokensSaida: 500);

        Assert.Equal(0.00034m, custo.Entrada);
        Assert.Equal(0.0008m, custo.Saida);
        Assert.Equal(0.00114m, custo.Total);
        Assert.Equal("USD", custo.Moeda);
        Assert.Equal("precos-teste.v1", custo.VersaoTabela);
    }

    [Fact]
    public void Sem_preco_de_cache_usa_o_preco_de_entrada()
    {
        var custo = Calculadora.Calcular("modelo-sem-cache", 1000, 400, 0);

        Assert.Equal(0.002m, custo.Entrada);
        Assert.Equal(0m, custo.Saida);
    }

    [Fact]
    public void Arredonda_em_8_casas_para_longe_do_zero()
    {
        // 1 × 0,025 ÷ 1.000.000 = 0,000000025 → 0,00000003 (o arredondamento bancário daria 0,00000002).
        var custo = Calculadora.Calcular("modelo-meio", 1, null, 1);

        Assert.Equal(0.00000003m, custo.Entrada);
        Assert.Equal(0.00000006m, custo.Total);
    }

    [Fact]
    public void Modelo_sem_preco_tem_custo_nulo()
    {
        var custo = Calculadora.Calcular("modelo-desconhecido", 1000, null, 100);

        Assert.Null(custo.Entrada);
        Assert.Null(custo.Saida);
        Assert.Null(custo.Total);
        Assert.Null(custo.Moeda);
        Assert.Equal("precos-teste.v1", custo.VersaoTabela);
    }

    [Fact]
    public void Tokens_ausentes_geram_custo_nulo_e_nunca_zero()
    {
        var semEntrada = Calculadora.Calcular("modelo-a", null, null, 100);
        var semNada = Calculadora.Calcular("modelo-a", null, null, null);

        Assert.Null(semEntrada.Entrada);
        Assert.Equal(0.00016m, semEntrada.Saida);
        Assert.Null(semEntrada.Total);
        Assert.Null(semNada.Total);
        Assert.Null(semNada.Moeda);
    }

    [Theory]
    [InlineData("modelo-a-2025-04-14", true)]
    [InlineData("modelo-a", true)]
    [InlineData("modelo-a-mini", false)]
    [InlineData("modelo-a-2025-04", false)]
    [InlineData("", false)]
    public void Aceita_o_sufixo_de_data_devolvido_pela_OpenAI(string modelo, bool temPreco)
    {
        Assert.Equal(temPreco, Calculadora.Calcular(modelo, 10, null, 10).Total is not null);
    }

    [Fact]
    public void Tabela_versionada_carrega_os_modelos_candidatos()
    {
        var tabela = TabelaPrecos.Carregar(Path.Combine(AppContext.BaseDirectory, "Catalogos", "precos-openai.v1.json"));

        Assert.Equal("precos-openai.v1", tabela.Versao);
        Assert.Equal("USD", tabela.Moeda);
        Assert.True(tabela.TentarObter("gpt-4.1-mini", out var preco));
        Assert.Equal(new PrecoModelo(0.40m, 0.10m, 1.60m), preco);
        Assert.True(tabela.TentarObter("gpt-5-mini", out _));
        Assert.True(tabela.TentarObter("gpt-6-luna", out _));
    }
}
