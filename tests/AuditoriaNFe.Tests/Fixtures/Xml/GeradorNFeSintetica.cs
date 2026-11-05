using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace AuditoriaNFe.Tests.Fixtures.Xml;

/// <summary>Item da NF-e sintética; os tributos são calculados pelo gerador.</summary>
public sealed record ItemSintetico(string Descricao, decimal Quantidade, decimal ValorUnitario, string Ncm = "48201000", string Cfop = "5102");

/// <summary>Parâmetros de uma NF-e sintética; os valores padrão formam um documento mínimo válido.</summary>
public sealed record OpcoesNFeSintetica
{
    public string CnpjEmitente { get; init; } = GeradorNFeSintetica.Cnpj("112223330001");
    public string CnpjDestinatario { get; init; } = GeradorNFeSintetica.Cnpj("445556660001");
    public string Modelo { get; init; } = "55";
    public string VersaoLeiaute { get; init; } = "4.00";
    public int Serie { get; init; } = 1;
    public int Numero { get; init; } = 1;
    public DateTimeOffset DhEmi { get; init; } = new(2026, 11, 2, 10, 0, 0, TimeSpan.FromHours(-3));

    /// <summary>Intervalo entre a emissão e o recebimento pela SEFAZ, no <c>dhRecbto</c> do protocolo.</summary>
    public TimeSpan AtrasoRecebimento { get; init; } = TimeSpan.FromMinutes(5);

    public bool ComProtocolo { get; init; } = true;
    public decimal AliquotaIcms { get; init; } = 18m;

    /// <summary>Conteúdo de <c>cEAN</c> e <c>cEANTrib</c>.</summary>
    public string Gtin { get; init; } = "SEM GTIN";

    public IReadOnlyList<ItemSintetico> Itens { get; init; } = [new("CADERNO SINTETICO", 2m, 50m)];

    /// <summary>Ajuste final do documento, para casos de erro ou diferenças entre pacotes de schema.</summary>
    public Action<XDocument>? Ajustar { get; init; }
}

/// <summary>
/// Monta NF-e modelo 55 sintéticas (P17, research R-23): CNPJs fictícios com dígitos verificadores,
/// chave de acesso com DV calculado, ambiente de homologação, itens e totais coerentes, com ou sem
/// <c>protNFe</c>. A <c>ds:Signature</c> é só estrutural, porque a assinatura não é validada na v1.
/// </summary>
public static class GeradorNFeSintetica
{
    public static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    public static readonly XNamespace Ds = "http://www.w3.org/2000/09/xmldsig#";

    private const string CodigoUf = "35";
    private const string CodigoMunicipio = "3550308";
    private const string FormatoData = "yyyy-MM-dd'T'HH:mm:sszzz";
    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;

    /// <summary>Diferenças do modelo mínimo exigidas pelo schema de cada pacote.</summary>
    private static readonly Dictionary<string, Func<OpcoesNFeSintetica, OpcoesNFeSintetica>> AjustesPorPacote = new(StringComparer.Ordinal)
    {
        // "SEM GTIN" só passou a ser aceito no cEAN a partir do PL_009_V4_2017_001.
        ["PL_009_V4_v1_40"] = opcoes => opcoes with { Gtin = string.Empty },
    };

    /// <summary>Modelo mínimo válido no schema do pacote, emitido em <paramref name="dhEmi"/>.</summary>
    public static OpcoesNFeSintetica OpcoesParaPacote(string pacote, DateTimeOffset dhEmi)
    {
        var opcoes = new OpcoesNFeSintetica { DhEmi = dhEmi };
        return AjustesPorPacote.TryGetValue(pacote, out var ajustar) ? ajustar(opcoes) : opcoes;
    }

    public static XDocument Gerar(OpcoesNFeSintetica? opcoes = null)
    {
        opcoes ??= new OpcoesNFeSintetica();
        var chave = ChaveAcesso(opcoes);
        var nfe = new XElement(Nfe + "NFe", InfNFe(opcoes, chave), Assinatura(chave));

        var raiz = opcoes.ComProtocolo
            ? new XElement(Nfe + "nfeProc", new XAttribute("versao", opcoes.VersaoLeiaute), nfe, Protocolo(opcoes, chave))
            : nfe;
        var documento = new XDocument(new XDeclaration("1.0", "UTF-8", null), raiz);
        opcoes.Ajustar?.Invoke(documento);
        return documento;
    }

    public static string GerarTexto(OpcoesNFeSintetica? opcoes = null)
    {
        var documento = Gerar(opcoes);
        return documento.Declaration + documento.ToString(SaveOptions.DisableFormatting);
    }

    public static byte[] GerarBytes(OpcoesNFeSintetica? opcoes = null) => Encoding.UTF8.GetBytes(GerarTexto(opcoes));

    public static string ChaveAcesso(OpcoesNFeSintetica opcoes)
    {
        var semDv = string.Concat(
            CodigoUf,
            opcoes.DhEmi.ToString("yyMM", Invariante),
            opcoes.CnpjEmitente,
            opcoes.Modelo.PadLeft(2, '0'),
            opcoes.Serie.ToString("D3", Invariante),
            opcoes.Numero.ToString("D9", Invariante),
            "1",
            CodigoNumerico(opcoes.Numero));
        return semDv + DigitoChave(semDv);
    }

    /// <summary>Módulo 11 com pesos de 2 a 9 da direita para a esquerda; restos 0 e 1 geram 0.</summary>
    public static int DigitoChave(string chaveSemDigito)
    {
        var soma = 0;
        var peso = 2;
        for (var i = chaveSemDigito.Length - 1; i >= 0; i--)
        {
            soma += (chaveSemDigito[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>CNPJ numérico com os dois dígitos verificadores a partir dos 12 primeiros dígitos.</summary>
    public static string Cnpj(string base12)
    {
        if (base12.Length != 12 || !base12.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Informe os 12 primeiros dígitos do CNPJ.", nameof(base12));
        }

        var com13 = base12 + DigitoCnpj(base12, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        return com13 + DigitoCnpj(com13, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
    }

    private static int DigitoCnpj(string digitos, int[] pesos)
    {
        var soma = digitos.Select((c, i) => (c - '0') * pesos[i]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string CodigoNumerico(int numero)
    {
        var codigo = (int)((numero * 7919L + 13) % 100_000_000);
        var texto = codigo.ToString("D8", Invariante);
        return texto == numero.ToString("D8", Invariante) ? (codigo + 1).ToString("D8", Invariante) : texto;
    }

    private static XElement InfNFe(OpcoesNFeSintetica opcoes, string chave)
    {
        var itens = opcoes.Itens.Select((item, indice) => Item(item, indice + 1, opcoes)).ToList();
        var vProd = itens.Sum(i => i.Valores.Produto);
        var vBc = itens.Sum(i => i.Valores.Produto);
        var vIcms = itens.Sum(i => i.Valores.Icms);
        var vPis = itens.Sum(i => i.Valores.Pis);
        var vCofins = itens.Sum(i => i.Valores.Cofins);

        return new XElement(Nfe + "infNFe",
            new XAttribute("versao", opcoes.VersaoLeiaute),
            new XAttribute("Id", "NFe" + chave),
            new XElement(Nfe + "ide",
                E("cUF", CodigoUf),
                E("cNF", chave.Substring(35, 8)),
                E("natOp", "VENDA DE MERCADORIA"),
                E("mod", opcoes.Modelo),
                E("serie", opcoes.Serie.ToString(Invariante)),
                E("nNF", opcoes.Numero.ToString(Invariante)),
                E("dhEmi", opcoes.DhEmi.ToString(FormatoData, Invariante)),
                E("tpNF", "1"),
                E("idDest", "1"),
                E("cMunFG", CodigoMunicipio),
                E("tpImp", "1"),
                E("tpEmis", "1"),
                E("cDV", chave[^1..]),
                E("tpAmb", "2"),
                E("finNFe", "1"),
                E("indFinal", "1"),
                E("indPres", "1"),
                E("procEmi", "0"),
                E("verProc", "SINTETICO 1.0")),
            new XElement(Nfe + "emit",
                E("CNPJ", opcoes.CnpjEmitente),
                E("xNome", "EMITENTE SINTETICO LTDA"),
                Endereco("enderEmit", "RUA FICTICIA"),
                E("IE", "111111111119"),
                E("CRT", "3")),
            new XElement(Nfe + "dest",
                E("CNPJ", opcoes.CnpjDestinatario),
                E("xNome", "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"),
                Endereco("enderDest", "AVENIDA FICTICIA"),
                E("indIEDest", "9")),
            itens.Select(i => i.Elemento),
            new XElement(Nfe + "total",
                new XElement(Nfe + "ICMSTot",
                    E("vBC", Valor(vBc)),
                    E("vICMS", Valor(vIcms)),
                    E("vICMSDeson", Valor(0)),
                    E("vFCP", Valor(0)),
                    E("vBCST", Valor(0)),
                    E("vST", Valor(0)),
                    E("vFCPST", Valor(0)),
                    E("vFCPSTRet", Valor(0)),
                    E("vProd", Valor(vProd)),
                    E("vFrete", Valor(0)),
                    E("vSeg", Valor(0)),
                    E("vDesc", Valor(0)),
                    E("vII", Valor(0)),
                    E("vIPI", Valor(0)),
                    E("vIPIDevol", Valor(0)),
                    E("vPIS", Valor(vPis)),
                    E("vCOFINS", Valor(vCofins)),
                    E("vOutro", Valor(0)),
                    E("vNF", Valor(vProd)))),
            new XElement(Nfe + "transp", E("modFrete", "9")),
            new XElement(Nfe + "pag",
                new XElement(Nfe + "detPag", E("tPag", "01"), E("vPag", Valor(vProd)))));
    }

    private sealed record ValoresItem(decimal Produto, decimal Icms, decimal Pis, decimal Cofins);

    private static (XElement Elemento, ValoresItem Valores) Item(ItemSintetico item, int numero, OpcoesNFeSintetica opcoes)
    {
        var aliquotaIcms = opcoes.AliquotaIcms;
        var vProd = Arredondar(item.Quantidade * item.ValorUnitario);
        var valores = new ValoresItem(
            vProd,
            Arredondar(vProd * aliquotaIcms / 100m),
            Arredondar(vProd * 1.65m / 100m),
            Arredondar(vProd * 7.60m / 100m));

        var elemento = new XElement(Nfe + "det",
            new XAttribute("nItem", numero.ToString(Invariante)),
            new XElement(Nfe + "prod",
                E("cProd", $"SINT{numero:D4}"),
                E("cEAN", opcoes.Gtin),
                E("xProd", item.Descricao),
                E("NCM", item.Ncm),
                E("CFOP", item.Cfop),
                E("uCom", "UN"),
                E("qCom", item.Quantidade.ToString("0.0000", Invariante)),
                E("vUnCom", item.ValorUnitario.ToString("0.0000", Invariante)),
                E("vProd", Valor(vProd)),
                E("cEANTrib", opcoes.Gtin),
                E("uTrib", "UN"),
                E("qTrib", item.Quantidade.ToString("0.0000", Invariante)),
                E("vUnTrib", item.ValorUnitario.ToString("0.0000", Invariante)),
                E("indTot", "1")),
            new XElement(Nfe + "imposto",
                new XElement(Nfe + "ICMS",
                    new XElement(Nfe + "ICMS00",
                        E("orig", "0"),
                        E("CST", "00"),
                        E("modBC", "3"),
                        E("vBC", Valor(vProd)),
                        E("pICMS", Valor(aliquotaIcms)),
                        E("vICMS", Valor(valores.Icms)))),
                new XElement(Nfe + "PIS",
                    new XElement(Nfe + "PISAliq",
                        E("CST", "01"),
                        E("vBC", Valor(vProd)),
                        E("pPIS", Valor(1.65m)),
                        E("vPIS", Valor(valores.Pis)))),
                new XElement(Nfe + "COFINS",
                    new XElement(Nfe + "COFINSAliq",
                        E("CST", "01"),
                        E("vBC", Valor(vProd)),
                        E("pCOFINS", Valor(7.60m)),
                        E("vCOFINS", Valor(valores.Cofins))))));

        return (elemento, valores);
    }

    private static XElement Endereco(string nome, string logradouro) =>
        new(Nfe + nome,
            E("xLgr", logradouro),
            E("nro", "100"),
            E("xBairro", "CENTRO"),
            E("cMun", CodigoMunicipio),
            E("xMun", "SAO PAULO"),
            E("UF", "SP"),
            E("CEP", "01001000"),
            E("cPais", "1058"),
            E("xPais", "BRASIL"));

    private static XElement Assinatura(string chave) =>
        new(Ds + "Signature",
            new XElement(Ds + "SignedInfo",
                new XElement(Ds + "CanonicalizationMethod", new XAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315")),
                new XElement(Ds + "SignatureMethod", new XAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#rsa-sha1")),
                new XElement(Ds + "Reference",
                    new XAttribute("URI", "#NFe" + chave),
                    new XElement(Ds + "Transforms",
                        new XElement(Ds + "Transform", new XAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature")),
                        new XElement(Ds + "Transform", new XAttribute("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315"))),
                    new XElement(Ds + "DigestMethod", new XAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#sha1")),
                    new XElement(Ds + "DigestValue", Base64Sintetico("digest", chave, 20)))),
            new XElement(Ds + "SignatureValue", Base64Sintetico("assinatura", chave, 256)),
            new XElement(Ds + "KeyInfo",
                new XElement(Ds + "X509Data",
                    new XElement(Ds + "X509Certificate", Base64Sintetico("certificado", chave, 512)))));

    private static XElement Protocolo(OpcoesNFeSintetica opcoes, string chave) =>
        new(Nfe + "protNFe",
            new XAttribute("versao", opcoes.VersaoLeiaute),
            new XElement(Nfe + "infProt",
                E("tpAmb", "2"),
                E("verAplic", "SINTETICO-1.0"),
                E("chNFe", chave),
                E("dhRecbto", (opcoes.DhEmi + opcoes.AtrasoRecebimento).ToString(FormatoData, Invariante)),
                E("nProt", "1" + CodigoUf + opcoes.DhEmi.ToString("yy", Invariante) + opcoes.Numero.ToString("D10", Invariante)),
                E("digVal", Base64Sintetico("digest", chave, 20)),
                E("cStat", "100"),
                E("xMotivo", "Autorizado o uso da NF-e")));

    /// <summary>Bytes determinísticos e sem valor criptográfico, só para preencher a estrutura.</summary>
    private static string Base64Sintetico(string finalidade, string chave, int tamanho)
    {
        var bytes = new byte[tamanho];
        var semente = SHA256.HashData(Encoding.UTF8.GetBytes(finalidade + "|" + chave));
        for (var i = 0; i < tamanho; i++)
        {
            bytes[i] = semente[i % semente.Length];
        }

        return Convert.ToBase64String(bytes);
    }

    private static XElement E(string nome, string valor) => new(Nfe + nome, valor);

    private static string Valor(decimal valor) => valor.ToString("0.00", Invariante);

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
