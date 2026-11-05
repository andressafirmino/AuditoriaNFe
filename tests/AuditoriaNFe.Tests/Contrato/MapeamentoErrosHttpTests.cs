using System.Text.Json;
using Api.Agente.Erros;
using AuditoriaNFe.Application.Contratos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AuditoriaNFe.Tests.Contrato;

public sealed class MapeamentoErrosHttpTests
{
    [Theory]
    [InlineData("ERR-001", 400)]
    [InlineData("ERR-003", 400)]
    [InlineData("ERR-007", 400)]
    [InlineData("ERR-021", 400)]
    [InlineData("ERR-002", 413)]
    [InlineData("ERR-005", 404)]
    [InlineData("ERR-006", 410)]
    [InlineData("ERR-020", 409)]
    [InlineData("ERR-022", 503)]
    [InlineData("ERR-025", 503)]
    [InlineData("ERR-024", 429)]
    public void Codigo_tem_o_status_do_plano(string codigo, int status)
    {
        Assert.Equal(status, MapeamentoErrosHttp.StatusPara(codigo));
    }

    [Fact]
    public void Apenas_os_codigos_do_plano_tem_status_proprio()
    {
        Assert.Equal(11, MapeamentoErrosHttp.CodigosMapeados.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => MapeamentoErrosHttp.StatusPara(CodigoErro.ProvedorIaIndisponivel));
        Assert.Throws<ArgumentOutOfRangeException>(() => MapeamentoErrosHttp.StatusPara(CodigoErro.NaoAutorizado));
    }

    [Fact]
    public async Task Erro_sem_execucao_usa_o_objeto_erro_com_mensagem_padrao()
    {
        var (status, _, corpo) = await Executar(MapeamentoErrosHttp.Erro(CodigoErro.SessaoNaoEncontrada));

        Assert.Equal(404, status);
        var raiz = corpo.RootElement;
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("execucaoId").ValueKind);
        Assert.Equal("ERR-005", raiz.GetProperty("erro").GetProperty("codigo").GetString());
        Assert.Equal(MensagensErro.Para(CodigoErro.SessaoNaoEncontrada), raiz.GetProperty("erro").GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task Erro_leva_mensagem_e_detalhes_informados()
    {
        var detalhes = new Dictionary<string, JsonElement>
        {
            ["limiteBytes"] = JsonSerializer.SerializeToElement(5_242_880),
            ["totalRecebidoBytes"] = JsonSerializer.SerializeToElement(6_000_000),
        };

        var (status, _, corpo) = await Executar(MapeamentoErrosHttp.Erro(
            CodigoErro.TamanhoExcedido, MensagensErro.TamanhoExcedido(5_242_880, 6_000_000), detalhes: detalhes));

        Assert.Equal(413, status);
        var erro = corpo.RootElement.GetProperty("erro");
        Assert.Equal(5_242_880, erro.GetProperty("detalhes").GetProperty("limiteBytes").GetInt32());
        Assert.Contains("6.000.000", erro.GetProperty("mensagem").GetString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(30.0, "30")]
    [InlineData(0.2, "1")]
    [InlineData(12.4, "13")]
    public async Task Limite_atingido_informa_Retry_After_em_segundos(double segundos, string esperado)
    {
        var (status, cabecalhos, corpo) = await Executar(MapeamentoErrosHttp.LimiteAtingido(TimeSpan.FromSeconds(segundos)));

        Assert.Equal(429, status);
        Assert.Equal(esperado, cabecalhos.RetryAfter.ToString());
        Assert.Equal("ERR-024", corpo.RootElement.GetProperty("erro").GetProperty("codigo").GetString());
        Assert.Contains(esperado, corpo.RootElement.GetProperty("erro").GetProperty("mensagem").GetString(), StringComparison.Ordinal);
    }

    private static async Task<(int Status, IHeaderDictionary Cabecalhos, JsonDocument Corpo)> Executar(IResult resultado)
    {
        var contexto = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        using var corpo = new MemoryStream();
        contexto.Response.Body = corpo;

        await resultado.ExecuteAsync(contexto);

        corpo.Position = 0;
        return (contexto.Response.StatusCode, contexto.Response.Headers, await JsonDocument.ParseAsync(corpo));
    }
}
