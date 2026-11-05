using AuditoriaNFe.Application.Abstracoes;

namespace AuditoriaNFe.Tests.Duplos;

/// <summary>Relógio que só avança quando o teste manda (research R-23).</summary>
public sealed class RelogioControlado(DateTimeOffset inicio) : IRelogio
{
    public static readonly DateTimeOffset InicioPadrao = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private readonly Lock _trava = new();
    private DateTimeOffset _agora = inicio;

    public RelogioControlado()
        : this(InicioPadrao)
    {
    }

    public DateTimeOffset Agora
    {
        get
        {
            lock (_trava)
            {
                return _agora;
            }
        }
    }

    public void Avancar(TimeSpan intervalo)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalo, TimeSpan.Zero);
        lock (_trava)
        {
            _agora += intervalo;
        }
    }

    public void Definir(DateTimeOffset instante)
    {
        lock (_trava)
        {
            _agora = instante;
        }
    }
}
