using System.Xml.Linq;

namespace AuditoriaNFe.Tests.Arquitetura;

internal static class Repositorio
{
    public static string Raiz()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (diretorio is not null && !File.Exists(Path.Combine(diretorio.FullName, "AgenteAuditoriaNFe.slnx")))
        {
            diretorio = diretorio.Parent;
        }

        return diretorio?.FullName ?? throw new InvalidOperationException("Raiz do repositório não encontrada.");
    }

    public static XDocument Projeto(string nome) =>
        XDocument.Load(Path.Combine(Raiz(), "src", nome, $"{nome}.csproj"));
}
