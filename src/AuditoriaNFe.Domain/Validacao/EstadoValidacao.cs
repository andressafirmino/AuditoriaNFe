namespace AuditoriaNFe.Domain.Validacao;

/// <summary>
/// Estado de uma validação: <c>Realizada</c> (com ou sem achados) ou <c>NaoRealizada</c> com motivo
/// (data-model §3; FR-015, ERR-023).
/// </summary>
public sealed record EstadoValidacao
{
    private EstadoValidacao(SituacaoValidacao situacao, string? motivo)
    {
        Situacao = situacao;
        Motivo = motivo;
    }

    public static EstadoValidacao Realizada { get; } = new(SituacaoValidacao.Realizada, null);

    public SituacaoValidacao Situacao { get; }

    /// <summary>Preenchido só em <see cref="SituacaoValidacao.NaoRealizada"/>.</summary>
    public string? Motivo { get; }

    public static EstadoValidacao NaoRealizada(string motivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(motivo);
        return new(SituacaoValidacao.NaoRealizada, motivo.Trim());
    }

    /// <summary>Forma textual do contrato: "Realizada" ou "NaoRealizada — motivo".</summary>
    public override string ToString() =>
        Situacao == SituacaoValidacao.Realizada ? "Realizada" : $"NaoRealizada — {Motivo}";
}
