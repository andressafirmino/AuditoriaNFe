using AuditoriaNFe.Application.Abstracoes;

namespace AuditoriaNFe.Infrastructure.Comum;

public sealed class RelogioSistema(TimeProvider timeProvider) : IRelogio
{
    public DateTimeOffset Agora => timeProvider.GetUtcNow();
}
