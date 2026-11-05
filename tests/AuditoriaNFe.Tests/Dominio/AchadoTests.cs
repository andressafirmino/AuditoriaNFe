using AuditoriaNFe.Domain.Validacao;

namespace AuditoriaNFe.Tests.Dominio;

public sealed class AchadoTests
{
    private static readonly Guid Id = Guid.CreateVersion7();

    [Theory]
    [InlineData(Severidade.Media)]
    [InlineData(Severidade.Baixa)]
    [InlineData(Severidade.Informativa)]
    public void Erro_estrutural_exige_severidade_alta(Severidade severidade)
    {
        Assert.Throws<ArgumentException>(() =>
            new Achado(Id, CategoriaAchado.ErroEstrutural, severidade, OrigemAchado.Deterministica, "XML malformado"));
    }

    [Fact]
    public void Erro_estrutural_com_severidade_alta_e_aceito()
    {
        var achado = new Achado(Id, CategoriaAchado.ErroEstrutural, Severidade.Alta, OrigemAchado.Deterministica, "XML malformado");

        Assert.Equal(Severidade.Alta, achado.Severidade);
        Assert.Empty(achado.Evidencias);
    }

    [Theory]
    [InlineData(null, "10.00", "R1", 1)]
    [InlineData("9.00", null, "R1", 1)]
    [InlineData("9.00", "10.00", null, 1)]
    [InlineData("9.00", "10.00", "R1", null)]
    [InlineData("9.00", "10.00", "R1", 0)]
    public void Inconsistencia_de_negocio_exige_valores_e_regra(
        string? informado, string? esperado, string? regraId, int? versaoRegra)
    {
        Assert.Throws<ArgumentException>(() => new Achado(
            Id, CategoriaAchado.InconsistenciaNegocio, Severidade.Media, OrigemAchado.Deterministica, "Total divergente",
            valorInformado: informado, valorEsperado: esperado, regraId: regraId, versaoRegra: versaoRegra));
    }

    [Fact]
    public void Inconsistencia_de_negocio_completa_e_aceita()
    {
        var achado = new Achado(
            Id, CategoriaAchado.InconsistenciaNegocio, Severidade.Media, OrigemAchado.Deterministica, "Total divergente",
            valorInformado: "9.00", valorEsperado: "10.00", regraId: "R1", versaoRegra: 1);

        Assert.Equal("R1", achado.RegraId);
    }

    [Fact]
    public void Indicio_da_IA_exige_evidencia()
    {
        Assert.Throws<ArgumentException>(() =>
            new Achado(Id, CategoriaAchado.IndicioIa, Severidade.Baixa, OrigemAchado.IA, "Indício"));

        var achado = new Achado(
            Id, CategoriaAchado.IndicioIa, Severidade.Baixa, OrigemAchado.IA, "Indício",
            evidencias: [Evidencia.Calculo(Guid.CreateVersion7())]);

        Assert.Single(achado.Evidencias);
    }

    [Fact]
    public void Erro_de_schema_exige_pacote()
    {
        Assert.Throws<ArgumentException>(() => Achado.ErroDeSchema(Id, "Elemento inesperado", " ", "a.xml", 3, 7));

        var achado = Achado.ErroDeSchema(Id, "Elemento inesperado", "PL_010e_v1.02", "a.xml", 3, 7);

        Assert.Equal(CategoriaAchado.ErroEstrutural, achado.Categoria);
        Assert.Equal(Severidade.Alta, achado.Severidade);
        Assert.Equal("PL_010e_v1.02", achado.PacoteSchema);
    }

    [Fact]
    public void Achado_exige_id_e_descricao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Achado(Guid.Empty, CategoriaAchado.FalhaTecnica, Severidade.Alta, OrigemAchado.Sistema, "Falha"));
        Assert.Throws<ArgumentException>(() =>
            new Achado(Id, CategoriaAchado.FalhaTecnica, Severidade.Alta, OrigemAchado.Sistema, " "));
    }

    [Fact]
    public void Evidencias_exigem_os_campos_do_tipo()
    {
        Assert.Throws<ArgumentException>(() => new Evidencia(TipoEvidencia.CampoXml, chaveAcesso: "1", caminho: "c"));
        Assert.Throws<ArgumentException>(() => new Evidencia(TipoEvidencia.RegraCatalogo, regraId: "R1", versaoCatalogo: "1"));
        Assert.Throws<ArgumentException>(() => new Evidencia(TipoEvidencia.TrechoManual, chunkId: "c1", documento: "MOC"));
        Assert.Throws<ArgumentException>(() => new Evidencia(TipoEvidencia.Calculo, calculoId: Guid.Empty));

        Assert.Equal(TipoEvidencia.CampoXml, Evidencia.CampoXml("1", "c", "v").Tipo);
        Assert.Equal(TipoEvidencia.RegraCatalogo, Evidencia.RegraCatalogo("R1", 1, "1").Tipo);
        Assert.Equal(TipoEvidencia.TrechoManual, Evidencia.TrechoManual("c1", "MOC", "7.0").Tipo);
    }

    [Fact]
    public void Estado_de_validacao_nao_realizada_exige_motivo()
    {
        Assert.Throws<ArgumentException>(() => EstadoValidacao.NaoRealizada(" "));
        Assert.Equal("NaoRealizada — layout inválido", EstadoValidacao.NaoRealizada("layout inválido").ToString());
        Assert.Equal("Realizada", EstadoValidacao.Realizada.ToString());
    }
}
