using System.Text.Json;
using AuditoriaNFe.Infrastructure.Ia.Simulacao;
using Microsoft.Extensions.AI;

namespace AuditoriaNFe.Tests.Aplicacao.Consumo;

public sealed class ChatClientSimuladoTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("")]
    public void E_recusado_fora_de_Development(string ambiente)
    {
        Assert.Throws<InvalidOperationException>(() => new ChatClientSimulado(ambiente));
    }

    [Fact]
    public async Task Resposta_se_declara_simulada_e_nao_informa_tokens()
    {
        using var cliente = new ChatClientSimulado("Development");

        var resposta = await cliente.GetResponseAsync("pergunta", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("simulado", resposta.ModelId);
        Assert.Null(resposta.Usage);
        Assert.Contains("simulada", resposta.Text, StringComparison.Ordinal);
        Assert.Equal("Simulado", cliente.GetService<ChatClientMetadata>()?.ProviderName);
    }

    [Fact]
    public async Task Pedido_em_JSON_recebe_JSON_marcado_como_simulado()
    {
        using var cliente = new ChatClientSimulado("Development");

        var resposta = await cliente.GetResponseAsync(
            "pergunta", new ChatOptions { ResponseFormat = ChatResponseFormat.Json }, TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(resposta.Text);
        Assert.True(json.RootElement.GetProperty("simulado").GetBoolean());
    }
}
