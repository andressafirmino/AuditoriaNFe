namespace AuditoriaNFe.Application.Abstracoes;

/// <summary>
/// Armazenamento de sessões e execuções indisponível ou sem resposta dentro do tempo configurado
/// (ERR-025; constitution 5.4). A mensagem não traz detalhes internos.
/// </summary>
public sealed class ArmazenamentoIndisponivelException : Exception
{
    public ArmazenamentoIndisponivelException()
        : base("Armazenamento de sessões e execuções indisponível.")
    {
    }

    public ArmazenamentoIndisponivelException(Exception innerException)
        : base("Armazenamento de sessões e execuções indisponível.", innerException)
    {
    }
}
