using AuditoriaNFe.Application.Configuracao;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuditoriaNFe.Infrastructure.Persistencia.Postgres;

/// <summary>Registro de consumo no schema <c>consumo</c> do banco <c>auditoria_agente</c> (ADR-0006).</summary>
public sealed class ConsumoDbContext(DbContextOptions<ConsumoDbContext> options) : DbContext(options)
{
    public const string Schema = "consumo";
    public const string NomeConnectionString = "Consumo";

    public DbSet<ChamadaProvedor> ChamadasProvedor => Set<ChamadaProvedor>();

    /// <summary>
    /// Npgsql com <c>Persistencia:TimeoutPostgres</c> na conexão e no comando, e o histórico de
    /// migrações no schema <c>consumo</c> (o usuário da aplicação não tem acesso ao <c>public</c>).
    /// </summary>
    public static void Configurar(DbContextOptionsBuilder builder, string connectionString, PersistenciaOpcoes opcoes)
    {
        var timeout = opcoes.TimeoutPostgres ?? throw new InvalidOperationException("Persistencia:TimeoutPostgres é obrigatório.");
        var segundos = Math.Max(1, (int)Math.Ceiling(timeout.TotalSeconds));
        var conexao = new NpgsqlConnectionStringBuilder(connectionString) { Timeout = segundos, CommandTimeout = segundos };

        builder.UseNpgsql(conexao.ConnectionString, npgsql => npgsql
            .CommandTimeout(segundos)
            .MigrationsHistoryTable("__EFMigrationsHistory", Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ChamadaProvedor>(chamada =>
        {
            chamada.ToTable("chamadas_provedor", tabela =>
            {
                tabela.HasCheckConstraint("ck_chamadas_provedor_tokens", "tokens_entrada >= 0 AND tokens_entrada_cache >= 0 AND tokens_saida >= 0 AND tokens_embedding >= 0");
                tabela.HasCheckConstraint("ck_chamadas_provedor_custos", "custo_entrada >= 0 AND custo_saida >= 0 AND custo_total >= 0");
                tabela.HasCheckConstraint("ck_chamadas_provedor_tentativa", "tentativa >= 1");
                tabela.HasCheckConstraint("ck_chamadas_provedor_duracao", "duracao_ms >= 0");
                tabela.HasCheckConstraint("ck_chamadas_provedor_tipo", "tipo_chamada IN ('Chat', 'Embedding')");
                tabela.HasCheckConstraint("ck_chamadas_provedor_status", "status IN ('Sucesso', 'Falha', 'Timeout', 'RespostaInvalida')");
                tabela.HasCheckConstraint("ck_chamadas_provedor_origem", "origem_valores IN ('InformadoProvedor', 'EstimadoSistema')");
                tabela.HasCheckConstraint("ck_chamadas_provedor_chave", "chave_isolamento ~ '^[0-9a-f]{64}$'");
            });

            chamada.HasKey(c => c.Id).HasName("pk_chamadas_provedor");
            chamada.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            chamada.Property(c => c.ExecucaoId).HasColumnName("execucao_id");
            chamada.Property(c => c.CorrelacaoId).HasColumnName("correlacao_id").HasMaxLength(128);
            chamada.Property(c => c.Servico).HasColumnName("servico").IsRequired();
            chamada.Property(c => c.Operacao).HasColumnName("operacao").IsRequired();
            chamada.Property(c => c.TipoChamada).HasColumnName("tipo_chamada").IsRequired();
            chamada.Property(c => c.Provedor).HasColumnName("provedor").IsRequired();
            chamada.Property(c => c.Modelo).HasColumnName("modelo").IsRequired();
            chamada.Property(c => c.Tentativa).HasColumnName("tentativa");
            chamada.Property(c => c.TokensEntrada).HasColumnName("tokens_entrada");
            chamada.Property(c => c.TokensEntradaCache).HasColumnName("tokens_entrada_cache");
            chamada.Property(c => c.TokensSaida).HasColumnName("tokens_saida");
            chamada.Property(c => c.TokensEmbedding).HasColumnName("tokens_embedding");
            chamada.Property(c => c.IniciadaEm).HasColumnName("iniciada_em");
            chamada.Property(c => c.DuracaoMs).HasColumnName("duracao_ms");
            chamada.Property(c => c.Status).HasColumnName("status").IsRequired();
            chamada.Property(c => c.CustoEntrada).HasColumnName("custo_entrada").HasPrecision(18, 8);
            chamada.Property(c => c.CustoSaida).HasColumnName("custo_saida").HasPrecision(18, 8);
            chamada.Property(c => c.CustoTotal).HasColumnName("custo_total").HasPrecision(18, 8);
            chamada.Property(c => c.Moeda).HasColumnName("moeda");
            chamada.Property(c => c.VersaoTabelaPrecos).HasColumnName("versao_tabela_precos");
            chamada.Property(c => c.OrigemValores).HasColumnName("origem_valores").IsRequired();
            chamada.Property(c => c.ChaveIsolamento).HasColumnName("chave_isolamento").IsRequired();

            chamada.HasIndex(c => c.ExecucaoId).HasDatabaseName("ix_chamadas_provedor_execucao_id");
            chamada.HasIndex(c => c.IniciadaEm).HasDatabaseName("ix_chamadas_provedor_iniciada_em");
            chamada.HasIndex(c => new { c.ChaveIsolamento, c.IniciadaEm }).HasDatabaseName("ix_chamadas_provedor_chave_iniciada_em");
        });
    }
}
