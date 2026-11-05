using AuditoriaNFe.Domain.Identificacao;

namespace AuditoriaNFe.Tests.Dominio;

public sealed class IdentificacaoSolicitanteTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void Ausentes_recebem_valores_padrao(string? usuario, string? chamador)
    {
        var identificacao = IdentificacaoSolicitante.Criar(usuario, chamador);

        Assert.Equal("anonimo", identificacao.UsuarioId);
        Assert.Equal("nao-informado", identificacao.ChamadorId);
        Assert.False(identificacao.UsuarioIdentificado);
    }

    [Fact]
    public void Usuario_informado_e_identificado()
    {
        var identificacao = IdentificacaoSolicitante.Criar("usuario.teste@exemplo", "sofia");

        Assert.True(identificacao.UsuarioIdentificado);
        Assert.Equal("usuario.teste@exemplo", identificacao.UsuarioId);
        Assert.Equal("sofia", identificacao.ChamadorId);
    }

    [Fact]
    public void Usuario_anonimo_explicito_nao_e_identificado()
    {
        Assert.False(IdentificacaoSolicitante.Criar("anonimo", "sofia").UsuarioIdentificado);
    }

    [Theory]
    [InlineData(null, null, "c6a4f16bf1910c2aeb1be080243f081c5cdad6c4510f06d148772f4000643f23")]
    [InlineData("sofia", "usuario.teste@exemplo", "f483983d0080fe458e13572fba50d930aa012dce580794a56d71265549038fe5")]
    public void Chave_de_isolamento_e_SHA256_estavel(string? chamador, string? usuario, string esperada)
    {
        var identificacao = IdentificacaoSolicitante.Criar(usuario, chamador);

        Assert.Equal(esperada, identificacao.ChaveIsolamento);
        Assert.Equal(identificacao.ChaveIsolamento, IdentificacaoSolicitante.Criar(usuario, chamador).ChaveIsolamento);
    }

    [Fact]
    public void Chave_distingue_chamadores_do_mesmo_usuario()
    {
        var a = IdentificacaoSolicitante.Criar("u1", "sofia");
        var b = IdentificacaoSolicitante.Criar("u1", "outro");

        Assert.NotEqual(a.ChaveIsolamento, b.ChaveIsolamento);
    }

    [Theory]
    [InlineData("com espaco")]
    [InlineData("acentuação")]
    [InlineData("barra/invalida")]
    [InlineData("quebra\n")]
    [InlineData(" ")]
    [InlineData("pipe|separador")]
    public void Formato_invalido_e_recusado(string valor)
    {
        Assert.False(IdentificacaoSolicitante.TentarCriar(valor, "sofia", out var identificacao));
        Assert.Null(identificacao);
        Assert.False(IdentificacaoSolicitante.TentarCriar("u1", valor, out _));
        Assert.Throws<ArgumentException>(() => IdentificacaoSolicitante.Criar(valor, null));
    }

    [Fact]
    public void Aceita_ate_128_caracteres()
    {
        Assert.True(IdentificacaoSolicitante.TentarCriar(new string('a', 128), null, out _));
        Assert.False(IdentificacaoSolicitante.TentarCriar(new string('a', 129), null, out _));
    }

    [Fact]
    public void Aceita_todos_os_caracteres_do_formato()
    {
        Assert.True(IdentificacaoSolicitante.TentarCriar("Az09._:@-", "Az09._:@-", out _));
    }
}
