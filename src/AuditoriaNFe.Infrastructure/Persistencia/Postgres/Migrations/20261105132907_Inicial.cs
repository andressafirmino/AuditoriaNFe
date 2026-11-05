using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditoriaNFe.Infrastructure.Persistencia.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "consumo");

            migrationBuilder.CreateTable(
                name: "chamadas_provedor",
                schema: "consumo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlacao_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    servico = table.Column<string>(type: "text", nullable: false),
                    operacao = table.Column<string>(type: "text", nullable: false),
                    tipo_chamada = table.Column<string>(type: "text", nullable: false),
                    provedor = table.Column<string>(type: "text", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    tentativa = table.Column<int>(type: "integer", nullable: false),
                    tokens_entrada = table.Column<int>(type: "integer", nullable: true),
                    tokens_entrada_cache = table.Column<int>(type: "integer", nullable: true),
                    tokens_saida = table.Column<int>(type: "integer", nullable: true),
                    tokens_embedding = table.Column<int>(type: "integer", nullable: true),
                    iniciada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duracao_ms = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    custo_entrada = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    custo_saida = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    custo_total = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    moeda = table.Column<string>(type: "text", nullable: true),
                    versao_tabela_precos = table.Column<string>(type: "text", nullable: true),
                    origem_valores = table.Column<string>(type: "text", nullable: false),
                    chave_isolamento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chamadas_provedor", x => x.id);
                    table.CheckConstraint("ck_chamadas_provedor_chave", "chave_isolamento ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_chamadas_provedor_custos", "custo_entrada >= 0 AND custo_saida >= 0 AND custo_total >= 0");
                    table.CheckConstraint("ck_chamadas_provedor_duracao", "duracao_ms >= 0");
                    table.CheckConstraint("ck_chamadas_provedor_origem", "origem_valores IN ('InformadoProvedor', 'EstimadoSistema')");
                    table.CheckConstraint("ck_chamadas_provedor_status", "status IN ('Sucesso', 'Falha', 'Timeout', 'RespostaInvalida')");
                    table.CheckConstraint("ck_chamadas_provedor_tentativa", "tentativa >= 1");
                    table.CheckConstraint("ck_chamadas_provedor_tipo", "tipo_chamada IN ('Chat', 'Embedding')");
                    table.CheckConstraint("ck_chamadas_provedor_tokens", "tokens_entrada >= 0 AND tokens_entrada_cache >= 0 AND tokens_saida >= 0 AND tokens_embedding >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_chamadas_provedor_chave_iniciada_em",
                schema: "consumo",
                table: "chamadas_provedor",
                columns: new[] { "chave_isolamento", "iniciada_em" });

            migrationBuilder.CreateIndex(
                name: "ix_chamadas_provedor_execucao_id",
                schema: "consumo",
                table: "chamadas_provedor",
                column: "execucao_id");

            migrationBuilder.CreateIndex(
                name: "ix_chamadas_provedor_iniciada_em",
                schema: "consumo",
                table: "chamadas_provedor",
                column: "iniciada_em");

            migrationBuilder.Sql("""
                CREATE VIEW consumo.vw_consumo_diario AS
                SELECT
                    (iniciada_em AT TIME ZONE 'UTC')::date AS dia,
                    modelo,
                    tipo_chamada,
                    count(*) AS chamadas,
                    count(*) FILTER (WHERE custo_total IS NULL) AS chamadas_sem_custo,
                    sum(tokens_entrada) AS tokens_entrada,
                    sum(tokens_entrada_cache) AS tokens_entrada_cache,
                    sum(tokens_saida) AS tokens_saida,
                    sum(tokens_embedding) AS tokens_embedding,
                    sum(custo_entrada) AS custo_entrada,
                    sum(custo_saida) AS custo_saida,
                    sum(custo_total) AS custo_total
                FROM consumo.chamadas_provedor
                GROUP BY 1, 2, 3;

                COMMENT ON VIEW consumo.vw_consumo_diario IS
                    'Consumo por dia (UTC), modelo e tipo de chamada (data-model §14). Somas ignoram valores NULL; chamadas_sem_custo conta as linhas sem custo.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS consumo.vw_consumo_diario;");

            migrationBuilder.DropTable(
                name: "chamadas_provedor",
                schema: "consumo");
        }
    }
}
