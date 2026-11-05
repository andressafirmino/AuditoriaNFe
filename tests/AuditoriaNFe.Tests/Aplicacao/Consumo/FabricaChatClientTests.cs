using System.Diagnostics.Metrics;
using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Application.Consumo;
using AuditoriaNFe.Domain.Sessoes;
using AuditoriaNFe.Infrastructure.Ia;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AuditoriaNFe.Tests.Aplicacao.Consumo;

public sealed class FabricaChatClientTests
{
    private const string Chave = "c6a4f16b00000000000000000000000000000000000000000000000000003f23";

    [Fact]
    public async Task Simulado_em_Development_passa_pelo_registro_com_origem_estimada()
    {
        using var cliente = Fabrica(ProvedorIa.Simulado, "Development").Criar();
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");

        var resposta = await cliente.GetResponseAsync("pergunta", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("simulado", resposta.ModelId);
        var resumo = escopo.Resumir();
        Assert.Equal(OrigemValoresConsumo.EstimadoSistema, resumo.Origem);
        Assert.Null(resumo.TokensEntrada);
        Assert.Null(resumo.CustoTotal);
    }

    [Fact]
    public void Simulado_fora_de_Development_e_recusado()
    {
        Assert.Throws<InvalidOperationException>(() => Fabrica(ProvedorIa.Simulado, "Production").Criar());
    }

    [Fact]
    public void OpenAI_sem_chave_ou_modelo_e_recusado()
    {
        Assert.Throws<InvalidOperationException>(() => Fabrica(ProvedorIa.OpenAI, "Development").Criar());
    }

    [Fact]
    public void OpenAI_com_chave_e_modelo_monta_o_cliente_sem_chamar_o_provedor()
    {
        using var cliente = Fabrica(ProvedorIa.OpenAI, "Production", new OpenAIOpcoes { ApiKey = "chave-ficticia", Modelo = "gpt-5-mini" }).Criar();

        Assert.Equal("gpt-5-mini", cliente.GetService<ChatClientMetadata>()?.DefaultModelId);
    }

    [Fact]
    public void Opcoes_nativas_sempre_desligam_o_armazenamento_e_preservam_o_que_ja_foi_definido()
    {
        var semFabrica = new ChatOptions();
        var comFabrica = new ChatOptions { RawRepresentationFactory = _ => new ChatCompletionOptions { EndUserId = "x" } };

        FabricaChatClient.AplicarSemArmazenamento(semFabrica);
        FabricaChatClient.AplicarSemArmazenamento(comFabrica);

        var nativasSem = Assert.IsType<ChatCompletionOptions>(semFabrica.RawRepresentationFactory!(null!));
        var nativasCom = Assert.IsType<ChatCompletionOptions>(comFabrica.RawRepresentationFactory!(null!));
        Assert.False(nativasSem.StoredOutputEnabled);
        Assert.False(nativasCom.StoredOutputEnabled);
        Assert.Equal("x", nativasCom.EndUserId);
    }

    private static FabricaChatClient Fabrica(ProvedorIa provedor, string ambiente, OpenAIOpcoes? openAi = null)
    {
        var persistencia = new PersistenciaOpcoes { TimeoutMongo = TimeSpan.FromSeconds(2), TimeoutPostgres = TimeSpan.FromSeconds(2) };
        var builder = new DbContextOptionsBuilder<ConsumoDbContext>();
        ConsumoDbContext.Configurar(builder, "Host=127.0.0.1;Port=1;Database=auditoria_agente;Username=teste", persistencia);
        var gravador = new GravadorConsumo(
            new FabricaContexto(builder.Options),
            new CalculadoraCusto(TabelaPrecos.Carregar(Path.Combine(AppContext.BaseDirectory, "Catalogos", "precos-openai.v1.json"))),
            new Relogio(),
            new Ids(),
            Options.Create(persistencia),
            new Medidores(),
            NullLogger<GravadorConsumo>.Instance);

        return new FabricaChatClient(
            Options.Create(new IaOpcoes { Provedor = provedor }),
            Options.Create(openAi ?? new OpenAIOpcoes()),
            gravador,
            NullLoggerFactory.Instance,
            ambiente);
    }

    private sealed class FabricaContexto(DbContextOptions<ConsumoDbContext> opcoes) : IDbContextFactory<ConsumoDbContext>
    {
        public ConsumoDbContext CreateDbContext() => new(opcoes);
    }

    private sealed class Relogio : IRelogio
    {
        public DateTimeOffset Agora => DateTimeOffset.UtcNow;
    }

    private sealed class Ids : IGeradorIds
    {
        public Guid Novo() => Guid.CreateVersion7();
    }

    private sealed class Medidores : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
