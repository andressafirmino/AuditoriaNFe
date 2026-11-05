using AuditoriaNFe.Application.Abstracoes;

namespace AuditoriaNFe.Infrastructure.Comum;

public sealed class GeradorIdsV7(IRelogio relogio) : IGeradorIds
{
    public Guid Novo() => Guid.CreateVersion7(relogio.Agora);
}
