using System.Text.RegularExpressions;

namespace AuditoriaNFe.Tests.Arquitetura;

/// <summary>Nenhuma chave de API, senha ou credencial versionada (constitution 5.1, research R-25).</summary>
public sealed partial class SemSegredosTests
{
    private static readonly HashSet<string> Extensoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".csproj", ".props", ".targets", ".slnx", ".json", ".md", ".yml", ".yaml",
        ".js", ".sql", ".ps1", ".sh", ".xml", ".config", ".http", ".txt", ".mdc",
    };

    private static readonly HashSet<string> NomesSemExtensao = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dockerfile", ".env.example", ".editorconfig", ".gitignore", ".dockerignore",
    };

    private static readonly HashSet<string> DiretoriosIgnorados = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".vs", "bin", "obj", "TestResults", "data", "node_modules",
    };

    private static readonly (string Regra, Regex Padrao)[] Regras =
    [
        ("chave da OpenAI", ChaveOpenAI()),
        ("token do GitHub", TokenGitHub()),
        ("chave privada", ChavePrivada()),
        ("ApiKey preenchida em JSON", ApiKeyEmJson()),
        ("senha em string de conexão", SenhaEmStringConexao()),
        ("credencial em URL do MongoDB", CredencialEmUrlMongo()),
        ("segredo atribuído em configuração", SegredoAtribuido()),
    ];

    [Fact]
    public void Repositorio_nao_contem_segredos()
    {
        var raiz = Repositorio.Raiz();
        var arquivos = Arquivos(new DirectoryInfo(raiz)).ToList();
        Assert.NotEmpty(arquivos);

        var ocorrencias = arquivos
            .SelectMany(arquivo => File.ReadLines(arquivo.FullName)
                .Select((texto, indice) => (Arquivo: arquivo.FullName, Linha: indice + 1, Texto: texto)))
            .SelectMany(linha => Regras
                .Where(regra => regra.Padrao.IsMatch(linha.Texto))
                .Select(regra => $"{Path.GetRelativePath(raiz, linha.Arquivo)}:{linha.Linha} ({regra.Regra})"))
            .ToList();

        Assert.True(
            ocorrencias.Count == 0,
            $"Possíveis segredos versionados:{Environment.NewLine}{string.Join(Environment.NewLine, ocorrencias)}");
    }

    [Theory]
    [InlineData("OpenAI__ApiKey=sk-proj-abcdefghijklmnopqrstuvwxyz0123")]
    [InlineData("token: ghp_abcdefghijklmnopqrstuvwxyz0123456789AB")]
    [InlineData("-----BEGIN RSA PRIVATE KEY-----")]
    [InlineData("  \"ApiKey\": \"valor-real\"")]
    [InlineData("Host=localhost;Username=agente;Password=Segredo123")]
    [InlineData("mongodb://agente:Segredo123@localhost:27017/auditoria_agente")]
    [InlineData("MONGO_APP_PASSWORD=Segredo123")]
    [InlineData("  POSTGRES_PASSWORD: Segredo123")]
    public void Regras_detectam_segredos(string linha)
    {
        Assert.Contains(Regras, regra => regra.Padrao.IsMatch(linha));
    }

    [Theory]
    [InlineData("OpenAI__ApiKey=")]
    [InlineData("MONGO_APP_PASSWORD=")]
    [InlineData("  \"ApiKey\": \"\"")]
    [InlineData("      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?Defina POSTGRES_PASSWORD no arquivo .env}")]
    [InlineData("Host=postgres;Password=${POSTGRES_APP_PASSWORD:?Defina POSTGRES_APP_PASSWORD no arquivo .env}")]
    [InlineData("mongodb://${MONGO_APP_USERNAME:?x}:${MONGO_APP_PASSWORD:?x}@mongodb:27017/auditoria_agente")]
    [InlineData("$\"mongodb://{_usuarioApp}:{_senhaApp}@{host}:{porta}/auditoria_agente\"")]
    public void Regras_aceitam_modelos_sem_valor(string linha)
    {
        Assert.DoesNotContain(Regras, regra => regra.Padrao.IsMatch(linha));
    }

    private static IEnumerable<FileInfo> Arquivos(DirectoryInfo diretorio)
    {
        foreach (var arquivo in diretorio.EnumerateFiles().Where(Varrido))
        {
            yield return arquivo;
        }

        foreach (var filho in diretorio.EnumerateDirectories().Where(d => !DiretoriosIgnorados.Contains(d.Name)))
        {
            foreach (var arquivo in Arquivos(filho))
            {
                yield return arquivo;
            }
        }
    }

    // Este arquivo fica fora da varredura: os casos de teste das regras são segredos de exemplo.
    // O .env local também fica fora: é ignorado pelo Git e guarda os segredos de quem desenvolve.
    private static bool Varrido(FileInfo arquivo) =>
        arquivo.Name != $"{nameof(SemSegredosTests)}.cs" &&
        (NomesSemExtensao.Contains(arquivo.Name) ||
         (Extensoes.Contains(arquivo.Extension) && !arquivo.Name.StartsWith(".env", StringComparison.OrdinalIgnoreCase)));

    [GeneratedRegex(@"sk-(proj-)?[A-Za-z0-9_-]{20,}")]
    private static partial Regex ChaveOpenAI();

    [GeneratedRegex(@"\bgh[pousr]_[A-Za-z0-9]{36,}")]
    private static partial Regex TokenGitHub();

    [GeneratedRegex(@"-----BEGIN ([A-Z]+ )?PRIVATE KEY-----")]
    private static partial Regex ChavePrivada();

    [GeneratedRegex("\"ApiKey\"\\s*:\\s*\"[^\"]+\"", RegexOptions.IgnoreCase)]
    private static partial Regex ApiKeyEmJson();

    [GeneratedRegex(@"\b(Password|Pwd)\s*=\s*(?!\$)[^;\s""']+", RegexOptions.IgnoreCase)]
    private static partial Regex SenhaEmStringConexao();

    [GeneratedRegex(@"mongodb(\+srv)?://[^:/\s@${]+:[^@\s${]+@", RegexOptions.IgnoreCase)]
    private static partial Regex CredencialEmUrlMongo();

    [GeneratedRegex(@"^\s*(?:[\w.-]*[_.-])?(PASSWORD|SENHA|API_?KEY|SECRET|TOKEN)(?:[_.-][\w.-]*)?\s*[:=][ \t]*(?!\$)[^\s#]", RegexOptions.IgnoreCase)]
    private static partial Regex SegredoAtribuido();
}
