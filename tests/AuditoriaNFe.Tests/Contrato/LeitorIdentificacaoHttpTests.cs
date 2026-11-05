using Api.Agente.Identificacao;
using AuditoriaNFe.Domain.Identificacao;
using Microsoft.AspNetCore.Http;

namespace AuditoriaNFe.Tests.Contrato;

public sealed class LeitorIdentificacaoHttpTests
{
    [Fact]
    public void Sem_cabecalhos_usa_anonimo_e_nao_informado()
    {
        Assert.True(LeitorIdentificacaoHttp.TentarLer(Requisicao(), out var identificacao));

        Assert.Equal(IdentificacaoSolicitante.UsuarioAnonimo, identificacao.UsuarioId);
        Assert.Equal(IdentificacaoSolicitante.ChamadorNaoInformado, identificacao.ChamadorId);
        Assert.False(identificacao.UsuarioIdentificado);
    }

    [Fact]
    public void Le_usuario_e_chamador_informados()
    {
        var requisicao = Requisicao(("X-Usuario-Id", "usuario.teste@exemplo"), ("X-Chamador-Id", "sofia"));

        Assert.True(LeitorIdentificacaoHttp.TentarLer(requisicao, out var identificacao));

        Assert.Equal("usuario.teste@exemplo", identificacao.UsuarioId);
        Assert.Equal("sofia", identificacao.ChamadorId);
        Assert.True(identificacao.UsuarioIdentificado);
    }

    [Theory]
    [InlineData("X-Usuario-Id", "usuário com espaço")]
    [InlineData("X-Chamador-Id", "sofia/producao")]
    [InlineData("X-Usuario-Id", "<script>")]
    public void Formato_invalido_e_rejeitado(string cabecalho, string valor)
    {
        Assert.False(LeitorIdentificacaoHttp.TentarLer(Requisicao((cabecalho, valor)), out _));
    }

    [Fact]
    public void Identificador_acima_de_128_caracteres_e_rejeitado()
    {
        Assert.False(LeitorIdentificacaoHttp.TentarLer(Requisicao(("X-Usuario-Id", new string('a', 129))), out _));
    }

    [Fact]
    public void Cabecalho_repetido_e_rejeitado()
    {
        var requisicao = new DefaultHttpContext().Request;
        requisicao.Headers.Append("X-Usuario-Id", new[] { "usuario-1", "usuario-2" });

        Assert.False(LeitorIdentificacaoHttp.TentarLer(requisicao, out _));
    }

    private static HttpRequest Requisicao(params (string Nome, string Valor)[] cabecalhos)
    {
        var requisicao = new DefaultHttpContext().Request;
        foreach (var (nome, valor) in cabecalhos)
        {
            requisicao.Headers[nome] = valor;
        }

        return requisicao;
    }
}
