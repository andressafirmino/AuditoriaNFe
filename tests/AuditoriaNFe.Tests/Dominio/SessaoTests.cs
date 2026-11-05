using AuditoriaNFe.Domain.Identificacao;
using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Tests.Dominio;

public sealed class SessaoTests
{
    private const string Chave = "35261112345678000195550010000000011000000010";
    private const string ChaveAlfanumerica = "352611AB3CD45EF00195550010000000011000000010";
    private static readonly string Hash = new('a', 64);
    private static readonly string OutroHash = new('b', 64);
    private static readonly DateTimeOffset Agora = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Expiracao = TimeSpan.FromHours(24);
    private static readonly IdentificacaoSolicitante Identificacao = IdentificacaoSolicitante.Criar("u1", "sofia");

    private static Sessao Nova() => Sessao.Criar(Guid.CreateVersion7(), Identificacao, Expiracao, Agora);

    [Fact]
    public void Nasce_ativa_e_pertence_a_chave_do_solicitante()
    {
        var sessao = Nova();

        Assert.Equal(EstadoSessao.Ativa, sessao.EstadoEm(Agora));
        Assert.True(sessao.PertenceA(Identificacao.ChaveIsolamento));
        Assert.False(sessao.PertenceA(IdentificacaoSolicitante.Criar("u2", "sofia").ChaveIsolamento));
    }

    [Fact]
    public void Expira_so_quando_a_inatividade_passa_do_prazo()
    {
        var sessao = Nova();

        Assert.Equal(EstadoSessao.Ativa, sessao.EstadoEm(Agora + Expiracao));
        Assert.Equal(EstadoSessao.Expirada, sessao.EstadoEm(Agora + Expiracao + TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void Atividade_renova_o_prazo()
    {
        var sessao = Nova();
        sessao.RegistrarAtividade(Agora.AddHours(20));

        Assert.Equal(EstadoSessao.Ativa, sessao.EstadoEm(Agora.AddHours(40)));
    }

    [Fact]
    public void Sessao_expirada_recusa_solicitacoes_e_grava_a_transicao()
    {
        var sessao = Nova();
        var depois = Agora.AddHours(25);

        Assert.Throws<InvalidOperationException>(() => sessao.RegistrarAtividade(depois));
        Assert.True(sessao.AtualizarExpiracao(depois));
        Assert.Equal(EstadoSessao.Expirada, sessao.Estado);
        Assert.False(sessao.AtualizarExpiracao(depois));
    }

    [Fact]
    public void Encerrada_recusa_novas_solicitacoes()
    {
        var sessao = Nova();
        sessao.Encerrar(Agora.AddMinutes(1));

        Assert.Equal(EstadoSessao.Encerrada, sessao.EstadoEm(Agora.AddMinutes(2)));
        Assert.Equal(Agora.AddMinutes(1), sessao.EncerradaEm);
        Assert.Throws<InvalidOperationException>(() => sessao.RegistrarAtividade(Agora.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(() => sessao.Encerrar(Agora.AddMinutes(2)));
        Assert.False(sessao.AtualizarExpiracao(Agora.AddDays(2)));
    }

    [Fact]
    public void Classifica_documentos_por_chave_e_hash()
    {
        var sessao = Nova();
        Assert.Equal(SituacaoDocumentoSessao.Novo, sessao.ClassificarDocumento(Chave, Hash));

        sessao.RegistrarDocumento(Guid.CreateVersion7(), Chave, Hash, Agora);

        Assert.Equal(SituacaoDocumentoSessao.Duplicado, sessao.ClassificarDocumento(Chave, Hash.ToUpperInvariant()));
        Assert.Equal(SituacaoDocumentoSessao.Divergente, sessao.ClassificarDocumento(Chave, OutroHash));
        Assert.Throws<InvalidOperationException>(() => sessao.RegistrarDocumento(Guid.CreateVersion7(), Chave, OutroHash, Agora));
        Assert.Single(sessao.Documentos);
        Assert.Equal(Hash, sessao.HashesPorChave[Chave]);
    }

    [Fact]
    public void Aceita_chave_com_CNPJ_alfanumerico()
    {
        var sessao = Nova();

        sessao.RegistrarDocumento(Guid.CreateVersion7(), ChaveAlfanumerica, Hash, Agora);

        Assert.Contains(ChaveAlfanumerica, sessao.HashesPorChave.Keys);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("3526111234567800019555001000000001100000001a")]
    [InlineData("35261112345678000195550010000000011000000010X")]
    public void Recusa_chave_fora_do_formato(string chave)
    {
        Assert.Throws<ArgumentException>(() => Nova().ClassificarDocumento(chave, Hash));
    }

    [Fact]
    public void Execucoes_ficam_em_ordem_sem_repeticao()
    {
        var sessao = Nova();
        var a = Guid.CreateVersion7();
        var b = Guid.CreateVersion7();

        sessao.RegistrarExecucao(a, Agora);
        sessao.RegistrarExecucao(b, Agora);
        sessao.RegistrarExecucao(a, Agora);

        Assert.Equal([a, b], sessao.Execucoes);
    }

    [Fact]
    public void Criacao_exige_id_e_prazo_positivo()
    {
        Assert.Throws<ArgumentException>(() => Sessao.Criar(Guid.Empty, Identificacao, Expiracao, Agora));
        Assert.Throws<ArgumentOutOfRangeException>(() => Sessao.Criar(Guid.CreateVersion7(), Identificacao, TimeSpan.Zero, Agora));
    }
}
