namespace AuditoriaNFe.Domain.Validacao;

/// <summary>Categoria do achado (P04, FR-020; data-model §7).</summary>
public enum CategoriaAchado
{
    ErroEstrutural,
    InconsistenciaNegocio,
    IndicioIa,
    AusenciaEvidencias,
    FalhaTecnica,
}
