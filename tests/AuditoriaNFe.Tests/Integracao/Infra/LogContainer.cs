using DotNet.Testcontainers.Containers;

namespace AuditoriaNFe.Tests.Integracao.Infra;

/// <summary>
/// Inicia o container e, se ele não subir, anexa o fim do log à exceção. As credenciais dos
/// containers de teste são geradas a cada execução e descartadas.
/// </summary>
internal static class LogContainer
{
    private const int LinhasFinais = 40;

    public static async Task IniciarAsync(IContainer container, string imagem)
    {
        try
        {
            await container.StartAsync();
        }
        catch (Exception erro)
        {
            var (saida, erros) = await container.GetLogsAsync();
            throw new InvalidOperationException(
                $"O container {imagem} não iniciou.{Environment.NewLine}stdout:{Environment.NewLine}{Fim(saida)}{Environment.NewLine}stderr:{Environment.NewLine}{Fim(erros)}",
                erro);
        }
    }

    private static string Fim(string log) =>
        string.Join(Environment.NewLine, log.Split('\n').TakeLast(LinhasFinais));
}
