using Microsoft.Extensions.AI;

namespace AuditoriaNFe.Tests.Duplos;

public sealed class DuplosTests
{
    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    [Fact]
    public void Relogio_so_avanca_quando_mandado()
    {
        var relogio = new RelogioControlado();

        relogio.Avancar(TimeSpan.FromMinutes(5));

        Assert.Equal(RelogioControlado.InicioPadrao.AddMinutes(5), relogio.Agora);
        Assert.Throws<ArgumentOutOfRangeException>(() => relogio.Avancar(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public async Task Roteiro_e_seguido_em_ordem_e_registra_as_chamadas()
    {
        using var cliente = new ChatClientRoteirizado()
            .ResponderComUso("""{"valido":true}""", tokensEntrada: 100, tokensSaida: 20, tokensEntradaCache: 10)
            .Responder("não é JSON")
            .Lancar(new HttpRequestException("indisponível"));

        var primeira = await cliente.GetResponseAsync("pergunta 1", cancellationToken: Cancelamento);
        var segunda = await cliente.GetResponseAsync("pergunta 2", cancellationToken: Cancelamento);
        await Assert.ThrowsAsync<HttpRequestException>(() => cliente.GetResponseAsync("pergunta 3", cancellationToken: Cancelamento));

        Assert.Equal(100, primeira.Usage?.InputTokenCount);
        Assert.Equal(10, primeira.Usage?.CachedInputTokenCount);
        Assert.Equal("não é JSON", segunda.Text);
        Assert.Null(segunda.Usage);
        Assert.Equal(["pergunta 1", "pergunta 2", "pergunta 3"], cliente.Chamadas.Select(c => c.Mensagens.Single().Text));
        Assert.Equal(0, cliente.Restantes);
    }

    [Fact]
    public async Task Tempo_esgotado_so_termina_com_o_cancelamento()
    {
        using var cliente = new ChatClientRoteirizado().EsgotarTempo();
        using var limite = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cliente.GetResponseAsync("pergunta", cancellationToken: limite.Token));
    }

    [Fact]
    public async Task Chamada_alem_do_roteiro_falha()
    {
        using var cliente = new ChatClientRoteirizado();

        await Assert.ThrowsAsync<InvalidOperationException>(() => cliente.GetResponseAsync("pergunta", cancellationToken: Cancelamento));
    }
}
