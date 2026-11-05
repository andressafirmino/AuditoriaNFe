using AuditoriaNFe.Application.Configuracao;
using AuditoriaNFe.Infrastructure.Persistencia.Postgres;
using AuditoriaNFe.Tests.Integracao.Infra;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuditoriaNFe.Tests.Integracao;

public sealed class ConsumoDbContextTests(ContainerPostgres postgres) : IClassFixture<ContainerPostgres>, IAsyncLifetime
{
    private static readonly string Chave = new('a', 64);
    private static readonly DateTimeOffset Dia = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private static CancellationToken Cancelamento => TestContext.Current.CancellationToken;

    private ConsumoDbContext Contexto()
    {
        var builder = new DbContextOptionsBuilder<ConsumoDbContext>();
        ConsumoDbContext.Configurar(
            builder,
            postgres.ConnectionString,
            new PersistenciaOpcoes { TimeoutMongo = TimeSpan.FromSeconds(5), TimeoutPostgres = TimeSpan.FromSeconds(5) });
        return new ConsumoDbContext(builder.Options);
    }

    public async ValueTask InitializeAsync()
    {
        await using var contexto = Contexto();
        await contexto.Database.MigrateAsync(Cancelamento);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static ChamadaProvedor Chamada(string modelo, int? tokensEntrada = 100, decimal? custoTotal = 0.0001m) => new()
    {
        Id = Guid.CreateVersion7(),
        ExecucaoId = Guid.CreateVersion7(),
        Operacao = "AnaliseIa",
        TipoChamada = "Chat",
        Provedor = "OpenAI",
        Modelo = modelo,
        TokensEntrada = tokensEntrada,
        TokensSaida = tokensEntrada is null ? null : 10,
        IniciadaEm = Dia,
        DuracaoMs = 50,
        Status = "Sucesso",
        CustoTotal = custoTotal,
        Moeda = custoTotal is null ? null : "USD",
        OrigemValores = "InformadoProvedor",
        ChaveIsolamento = Chave,
    };

    [Fact]
    public async Task Migracao_com_usuario_da_aplicacao_cria_tabela_e_historico_no_schema_consumo()
    {
        await using var conexao = new NpgsqlConnection(postgres.ConnectionString);
        await conexao.OpenAsync(Cancelamento);
        await using var comando = new NpgsqlCommand(
            "SELECT table_schema || '.' || table_name FROM information_schema.tables WHERE table_schema IN ('consumo', 'public') ORDER BY 1", conexao);
        var tabelas = new List<string>();
        await using (var leitor = await comando.ExecuteReaderAsync(Cancelamento))
        {
            while (await leitor.ReadAsync(Cancelamento))
            {
                tabelas.Add(leitor.GetString(0));
            }
        }

        Assert.Equal(["consumo.__EFMigrationsHistory", "consumo.chamadas_provedor", "consumo.vw_consumo_diario"], tabelas);
    }

    [Fact]
    public async Task Tokens_e_custos_aceitam_NULL_e_recusam_negativos()
    {
        await using (var contexto = Contexto())
        {
            contexto.ChamadasProvedor.Add(Chamada("modelo-sem-preco", tokensEntrada: null, custoTotal: null));
            await contexto.SaveChangesAsync(Cancelamento);
        }

        await using (var contexto = Contexto())
        {
            contexto.ChamadasProvedor.Add(Chamada("modelo-x", tokensEntrada: -1));
            var excecao = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Cancelamento));
            Assert.Contains("ck_chamadas_provedor_tokens", excecao.InnerException!.Message, StringComparison.Ordinal);
        }

        await using (var contexto = Contexto())
        {
            contexto.ChamadasProvedor.Add(Chamada("modelo-x", custoTotal: -0.1m));
            await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Cancelamento));
        }
    }

    [Fact]
    public async Task Chave_de_isolamento_em_claro_e_recusada()
    {
        await using var contexto = Contexto();
        var chamada = Chamada("modelo-x");
        chamada.ChaveIsolamento = "usuario@exemplo";
        contexto.ChamadasProvedor.Add(chamada);

        var excecao = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Cancelamento));
        Assert.Contains("ck_chamadas_provedor_chave", excecao.InnerException!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Visao_diaria_soma_por_dia_modelo_e_tipo()
    {
        var modelo = $"modelo-{Guid.NewGuid():N}";
        await using (var contexto = Contexto())
        {
            contexto.ChamadasProvedor.AddRange(Chamada(modelo), Chamada(modelo), Chamada(modelo, custoTotal: null));
            await contexto.SaveChangesAsync(Cancelamento);
        }

        await using var conexao = new NpgsqlConnection(postgres.ConnectionString);
        await conexao.OpenAsync(Cancelamento);
        await using var comando = new NpgsqlCommand(
            "SELECT dia, chamadas, chamadas_sem_custo, tokens_entrada, custo_total FROM consumo.vw_consumo_diario WHERE modelo = @modelo AND tipo_chamada = 'Chat'",
            conexao);
        comando.Parameters.AddWithValue("modelo", modelo);
        await using var leitor = await comando.ExecuteReaderAsync(Cancelamento);

        Assert.True(await leitor.ReadAsync(Cancelamento));
        Assert.Equal(new DateOnly(2026, 11, 4), DateOnly.FromDateTime(leitor.GetDateTime(0)));
        Assert.Equal(3, leitor.GetInt64(1));
        Assert.Equal(1, leitor.GetInt64(2));
        Assert.Equal(300, leitor.GetInt64(3));
        Assert.Equal(0.0002m, leitor.GetDecimal(4));
        Assert.False(await leitor.ReadAsync(Cancelamento));
    }
}
