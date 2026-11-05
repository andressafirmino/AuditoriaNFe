using AuditoriaNFe.Application.Consumo;
using AuditoriaNFe.Domain.Sessoes;

namespace AuditoriaNFe.Tests.Aplicacao.Consumo;

public sealed class EscopoConsumoTests
{
    private const string Chave = "c6a4f16b00000000000000000000000000000000000000000000000000003f23";

    [Fact]
    public void Escopo_fica_disponivel_ate_ser_descartado_e_restaura_o_anterior()
    {
        Assert.Null(EscopoConsumo.Atual);

        using (var externo = EscopoConsumo.Iniciar(Guid.NewGuid(), "corr-1", Chave, "AnalisarNFe"))
        {
            using (var interno = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "ResponderPergunta"))
            {
                Assert.Same(interno, EscopoConsumo.Atual);
            }

            Assert.Same(externo, EscopoConsumo.Atual);
        }

        Assert.Null(EscopoConsumo.Atual);
    }

    [Fact]
    public async Task Escopo_flui_para_as_continuacoes_assincronas()
    {
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");

        var visto = await Task.Run(async () =>
        {
            await Task.Yield();
            return EscopoConsumo.Atual;
        });

        Assert.Same(escopo, visto);
    }

    [Fact]
    public void Tentativa_comeca_em_um_e_nao_aceita_zero()
    {
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");

        Assert.Equal(1, escopo.Tentativa);
        escopo.DefinirTentativa(2);
        Assert.Equal(2, escopo.Tentativa);
        Assert.Throws<ArgumentOutOfRangeException>(() => escopo.DefinirTentativa(0));
    }

    [Fact]
    public void Resumo_soma_as_chamadas_informadas()
    {
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");
        escopo.RegistrarChamada(new ConsumoChamada(1000, 200, 300, 0.001m, "USD", OrigemValoresConsumo.InformadoProvedor));
        escopo.RegistrarChamada(new ConsumoChamada(500, 0, 100, 0.0005m, "USD", OrigemValoresConsumo.InformadoProvedor));

        var resumo = escopo.Resumir();

        Assert.Equal(new ConsumoResumo(1500, 200, 400, 0.0015m, "USD", OrigemValoresConsumo.InformadoProvedor, true), resumo);
    }

    [Fact]
    public void Valor_ausente_em_uma_chamada_torna_o_total_desconhecido()
    {
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");
        escopo.RegistrarChamada(new ConsumoChamada(1000, null, 300, 0.001m, "USD", OrigemValoresConsumo.InformadoProvedor));
        escopo.RegistrarChamada(new ConsumoChamada(null, null, null, null, null, OrigemValoresConsumo.EstimadoSistema));

        var resumo = escopo.Resumir();

        Assert.Null(resumo.TokensEntrada);
        Assert.Null(resumo.TokensSaida);
        Assert.Null(resumo.CustoTotal);
        Assert.Null(resumo.Moeda);
        Assert.Equal(OrigemValoresConsumo.EstimadoSistema, resumo.Origem);
    }

    [Fact]
    public void Sem_chamadas_nao_ha_valores_e_falha_de_gravacao_marca_registro_incompleto()
    {
        using var escopo = EscopoConsumo.Iniciar(Guid.NewGuid(), null, Chave, "AnalisarNFe");

        Assert.Equal(new ConsumoResumo(null, null, null, null, null, null, true), escopo.Resumir());

        escopo.RegistrarFalhaGravacao();

        Assert.False(escopo.Resumir().RegistroCompleto);
    }
}
