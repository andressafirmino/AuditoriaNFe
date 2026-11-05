namespace AuditoriaNFe.Application.Abstracoes;

/// <summary>Data e hora do sistema, em UTC (P09). Substituível nos testes.</summary>
public interface IRelogio
{
    DateTimeOffset Agora { get; }
}
