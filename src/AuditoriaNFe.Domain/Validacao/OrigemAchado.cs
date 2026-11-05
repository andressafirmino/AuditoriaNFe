namespace AuditoriaNFe.Domain.Validacao;

/// <summary>Quem produziu o achado (data-model §7).</summary>
public enum OrigemAchado
{
    Deterministica,
    IA,
    Sistema,
}
