using AuditoriaNFe.Application.Abstracoes;
using AuditoriaNFe.Tests.Duplos;
using AuditoriaNFe.Tests.Integracao.Infra;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AuditoriaNFe.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public sealed class AmbienteIntegracaoTests(AmbienteIntegracao ambiente)
{
    [Fact]
    public void Host_de_teste_usa_os_duplos_atras_do_pipeline_de_IA()
    {
        var servicos = ambiente.Fabrica.Services;

        Assert.Same(ambiente.Relogio, servicos.GetRequiredService<IRelogio>());
        var cliente = servicos.GetRequiredService<IChatClient>();
        Assert.NotSame(ambiente.ChatClient, cliente);
        Assert.Same(ambiente.ChatClient, cliente.GetService<ChatClientRoteirizado>());
    }
}
