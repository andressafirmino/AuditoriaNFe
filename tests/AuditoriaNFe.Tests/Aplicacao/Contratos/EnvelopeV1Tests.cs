using System.Text.Json;
using AuditoriaNFe.Application.Contratos;
using AuditoriaNFe.Domain.Identificacao;
using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Tests.Aplicacao.Contratos;

public sealed class EnvelopeV1Tests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Envelope_de_execucao_rejeitada_tem_os_campos_obrigatorios_do_schema()
    {
        var execucao = Execucao.Receber(
            Guid.CreateVersion7(), IdentificacaoSolicitante.Criar(null, null), CanalExecucao.Direto,
            TipoExecucao.AnaliseLote, Agora, correlacaoId: "corr-1");
        execucao.Rejeitar(new ErroExecucao(CodigoErro.SolicitacaoSemConteudo, MensagensErro.Para(CodigoErro.SolicitacaoSemConteudo)), Agora);

        var json = JsonSerializer.SerializeToElement(EnvelopeV1.De(execucao, Agora, 12), OpcoesJsonContrato.Padrao);

        foreach (var campo in new[] { "versaoEnvelope", "versaoContratoIa", "execucaoId", "estado", "dataHora", "canal", "usuarioIdentificado", "etapas" })
        {
            Assert.True(json.TryGetProperty(campo, out _), $"Campo obrigatório ausente: {campo}");
        }

        Assert.Equal("1.0", json.GetProperty("versaoEnvelope").GetString());
        Assert.Equal("1.0", json.GetProperty("versaoContratoIa").GetString());
        Assert.Equal("Rejeitada", json.GetProperty("estado").GetString());
        Assert.Equal("Direto", json.GetProperty("canal").GetString());
        Assert.False(json.GetProperty("usuarioIdentificado").GetBoolean());
        Assert.Equal("corr-1", json.GetProperty("correlacaoId").GetString());
        Assert.Equal(9, json.GetProperty("etapas").GetArrayLength());
        Assert.Equal("NaoExecutada", json.GetProperty("etapas")[0].GetProperty("estado").GetString());
        Assert.Equal("ERR-001", json.GetProperty("erro").GetProperty("codigo").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("parecer").ValueKind);
        Assert.Equal(12, json.GetProperty("metadados").GetProperty("duracaoMs").GetInt64());
    }

    [Fact]
    public void Custo_e_serializado_como_texto_com_8_casas()
    {
        var consumo = ConsumoV1.De(new ConsumoResumo(100, null, 50, 0.000123m, "USD", OrigemValoresConsumo.InformadoProvedor, true));

        Assert.Equal("0.00012300", consumo.CustoEstimado);
        Assert.Equal("InformadoProvedor", consumo.Origem);
    }

    [Fact]
    public void Recomendacao_tem_rotulo_fixo()
    {
        var json = JsonSerializer.SerializeToElement(
            new RecomendacaoV1(Guid.CreateVersion7(), "PendenteAprovacao", null, null, []), OpcoesJsonContrato.Padrao);

        Assert.Equal("Recomendação demonstrativa", json.GetProperty("rotulo").GetString());
    }
}
