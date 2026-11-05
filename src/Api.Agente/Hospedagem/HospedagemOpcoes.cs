namespace Api.Agente.Hospedagem;

/// <summary>Modo de hospedagem do host (FR-052f, desvio D-08, ADR-0012).</summary>
public enum ModoHospedagem
{
    Completo,

    /// <summary>Só o Agent Card e a vivacidade; único modo aceito fora de <c>Development</c>.</summary>
    SomenteAgentCard,
}

public sealed class HospedagemOpcoes
{
    public const string Secao = "Hospedagem";
    public const string ChaveModo = $"{Secao}:{nameof(Modo)}";

    public ModoHospedagem Modo { get; set; } = ModoHospedagem.Completo;

    /// <summary>
    /// Lido antes de montar o host, porque decide o que é registrado. Só aceita os nomes do enum:
    /// um número fora da lista seria convertido pelo binder sem erro.
    /// </summary>
    public static ModoHospedagem LerModo(IConfiguration configuration)
    {
        var valor = configuration[ChaveModo]?.Trim();
        if (string.IsNullOrEmpty(valor))
        {
            return ModoHospedagem.Completo;
        }

        var nome = Enum.GetNames<ModoHospedagem>().FirstOrDefault(n => string.Equals(n, valor, StringComparison.OrdinalIgnoreCase));
        return nome is not null
            ? Enum.Parse<ModoHospedagem>(nome)
            : throw new InvalidOperationException(
                $"{ChaveModo} inválido: use {nameof(ModoHospedagem.Completo)} ou {nameof(ModoHospedagem.SomenteAgentCard)} (desvio D-08).");
    }
}
