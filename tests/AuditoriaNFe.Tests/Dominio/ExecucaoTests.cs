using AuditoriaNFe.Domain.Identificacao;
using AuditoriaNFe.Domain.Sessoes;
using AuditoriaNFe.Domain.Validacao;

namespace AuditoriaNFe.Tests.Dominio;

public sealed class ExecucaoTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);
    private static readonly IdentificacaoSolicitante Identificacao = IdentificacaoSolicitante.Criar("u1", "sofia");

    private static Execucao Nova(string? correlacao = null) =>
        Execucao.Receber(Guid.CreateVersion7(), Identificacao, CanalExecucao.Direto, TipoExecucao.AnaliseLote, Agora, correlacao);

    [Fact]
    public void Nasce_recebida_com_nove_etapas_nao_registradas()
    {
        var execucao = Nova();

        Assert.Equal(EstadoExecucao.Recebida, execucao.Estado);
        Assert.Equal(Enumerable.Range(1, 9), execucao.Etapas.Select(e => e.Numero));
        Assert.All(execucao.Etapas, e => Assert.False(e.Registrada));
        Assert.Equal(Identificacao.ChaveIsolamento, execucao.ChaveIsolamento);
        Assert.True(execucao.UsuarioIdentificado);
    }

    [Fact]
    public void Correlacao_aceita_ate_128_caracteres()
    {
        Assert.Equal(128, Nova(new string('c', 128)).CorrelacaoId!.Length);
        Assert.Throws<ArgumentException>(() => Nova(new string('c', 129)));
    }

    [Fact]
    public void Fluxo_completo_conclui_com_todas_as_etapas()
    {
        var execucao = Nova();
        execucao.IniciarProcessamento();
        for (var n = 1; n <= 9; n++)
        {
            if (n is 5 or 6)
            {
                execucao.RegistrarEtapa(n, EstadoEtapa.NaoAplicavel, justificativa: "Integração desabilitada.");
            }
            else
            {
                execucao.RegistrarEtapa(n, EstadoEtapa.Executada, Agora, Agora.AddMilliseconds(5));
            }
        }

        execucao.Concluir(Agora.AddSeconds(1));

        Assert.Equal(EstadoExecucao.Concluida, execucao.Estado);
        Assert.Equal(Agora.AddSeconds(1), execucao.ConcluidaEm);
    }

    [Fact]
    public void Concluir_com_etapa_nao_registrada_e_proibido()
    {
        var execucao = Nova();
        execucao.IniciarProcessamento();
        execucao.RegistrarEtapa(1, EstadoEtapa.Executada);

        Assert.Throws<InvalidOperationException>(() => execucao.Concluir(Agora));
    }

    [Theory]
    [InlineData(EstadoEtapa.NaoAplicavel)]
    [InlineData(EstadoEtapa.NaoExecutada)]
    public void Justificativa_e_obrigatoria(EstadoEtapa estado)
    {
        var execucao = Nova();

        Assert.Throws<ArgumentException>(() => execucao.RegistrarEtapa(5, estado));
        Assert.Throws<ArgumentException>(() => execucao.RegistrarEtapa(5, estado, justificativa: " "));
    }

    [Fact]
    public void Rejeicao_so_a_partir_de_recebida_e_com_codigos_do_diagrama()
    {
        var execucao = Nova();

        Assert.Throws<ArgumentException>(() => execucao.Rejeitar(new ErroExecucao("ERR-016", "Tempo esgotado."), Agora));

        execucao.Rejeitar(new ErroExecucao("ERR-002", "Lote acima do limite."), Agora);

        Assert.Equal(EstadoExecucao.Rejeitada, execucao.Estado);
        Assert.All(execucao.Etapas, e =>
        {
            Assert.Equal(EstadoEtapa.NaoExecutada, e.Estado);
            Assert.False(string.IsNullOrWhiteSpace(e.Justificativa));
        });
    }

    [Fact]
    public void Falha_registra_etapas_concluidas_e_nao_executadas()
    {
        var execucao = Nova();
        execucao.IniciarProcessamento();
        execucao.RegistrarEtapa(1, EstadoEtapa.Executada);
        execucao.RegistrarEtapa(2, EstadoEtapa.Executada);

        execucao.Falhar(new ErroExecucao("ERR-016", "Tempo esgotado."), Agora);

        Assert.Equal(EstadoExecucao.Falhou, execucao.Estado);
        Assert.Equal("ERR-016", execucao.Erro!.Codigo);
        Assert.Equal(EstadoEtapa.Executada, execucao.Etapa(2).Estado);
        Assert.All(execucao.Etapas.Skip(2), e => Assert.Equal(EstadoEtapa.NaoExecutada, e.Estado));
    }

    [Fact]
    public void Transicoes_proibidas_lancam()
    {
        var recebida = Nova();
        Assert.Throws<InvalidOperationException>(() => recebida.Concluir(Agora));
        Assert.Throws<InvalidOperationException>(() => recebida.Falhar(new ErroExecucao("ERR-016", "x"), Agora));

        var emProcessamento = Nova();
        emProcessamento.IniciarProcessamento();
        Assert.Throws<InvalidOperationException>(() => emProcessamento.IniciarProcessamento());
        Assert.Throws<InvalidOperationException>(() => emProcessamento.Rejeitar(new ErroExecucao("ERR-001", "x"), Agora));

        var rejeitada = Nova();
        rejeitada.Rejeitar(new ErroExecucao("ERR-001", "x"), Agora);
        Assert.Throws<InvalidOperationException>(() => rejeitada.IniciarProcessamento());
        Assert.Throws<InvalidOperationException>(() => rejeitada.RegistrarEtapa(1, EstadoEtapa.Executada));
    }

    [Fact]
    public void Etapa_fora_de_1_a_9_e_recusada()
    {
        var execucao = Nova();

        Assert.Throws<ArgumentOutOfRangeException>(() => execucao.RegistrarEtapa(0, EstadoEtapa.Executada));
        Assert.Throws<ArgumentOutOfRangeException>(() => execucao.RegistrarEtapa(10, EstadoEtapa.Executada));
    }

    [Theory]
    [InlineData("listar documentos")]
    [InlineData("valor=10.00")]
    [InlineData("")]
    public void Chamada_de_ferramenta_nao_aceita_texto_livre(string nome)
    {
        Assert.Throws<ArgumentException>(() =>
            new ChamadaFerramenta(nome, Agora, 1, StatusChamadaFerramenta.Sucesso));
        Assert.Throws<ArgumentException>(() =>
            new ChamadaFerramenta("listar_documentos", Agora, 1, StatusChamadaFerramenta.Falha, erro: nome == string.Empty ? "a b" : nome));
    }

    [Fact]
    public void Registra_ferramentas_e_trechos()
    {
        var execucao = Nova();
        execucao.RegistrarChamadaFerramenta(new ChamadaFerramenta("calcular_total", Agora, 3, StatusChamadaFerramenta.Sucesso, calculoId: Guid.CreateVersion7()));
        execucao.RegistrarTrechoConsultado(new TrechoConsultado("c1", "MOC", "7.0"));

        Assert.Single(execucao.ChamadasFerramentas);
        Assert.Single(execucao.TrechosConsultados);
    }

    [Theory]
    [InlineData("ERR-1")]
    [InlineData("err-001")]
    [InlineData("ERR-0001")]
    public void Codigo_de_erro_segue_o_formato(string codigo)
    {
        Assert.Throws<ArgumentException>(() => new ErroExecucao(codigo, "mensagem"));
    }
}
