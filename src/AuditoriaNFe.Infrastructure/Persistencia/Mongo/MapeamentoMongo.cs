using AuditoriaNFe.Domain.Sessoes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace AuditoriaNFe.Infrastructure.Persistencia.Mongo;

/// <summary>
/// Mapeamento das entidades do Domínio para BSON, registrado uma vez por processo: GUID em
/// representação padrão (binData 4), datas como BSON date em UTC, decimais como Decimal128, enums
/// como texto e nomes em camelCase (convenções de <c>infra/mongodb/init/01-colecoes.js</c>).
/// </summary>
internal static class MapeamentoMongo
{
    private static readonly Lock Trava = new();
    private static bool _registrado;

    public static void Registrar()
    {
        lock (Trava)
        {
            if (_registrado)
            {
                return;
            }

            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            BsonSerializer.TryRegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

            var convencoes = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            };
            ConventionRegistry.Register(
                "AuditoriaNFe",
                convencoes,
                tipo => tipo.Namespace?.StartsWith("AuditoriaNFe.", StringComparison.Ordinal) == true);

            BsonClassMap.TryRegisterClassMap<Sessao>(mapa =>
            {
                mapa.AutoMap();
                mapa.MapField("_documentos").SetElementName("documentos");
                mapa.MapField("_execucoes").SetElementName("execucoes");
                mapa.MapField("_hashesPorChave").SetElementName("hashesPorChave");
            });

            BsonClassMap.TryRegisterClassMap<Execucao>(mapa =>
            {
                mapa.AutoMap();
                mapa.MapField("_etapas").SetElementName("etapas");
                mapa.MapField("_chamadasFerramentas").SetElementName("chamadasFerramentas");
                mapa.MapField("_trechosConsultados").SetElementName("trechosConsultados");
                mapa.MapField("_versoes").SetElementName("versoes");
            });

            BsonClassMap.TryRegisterClassMap<EtapaExecucao>(mapa => mapa.AutoMap());

            _registrado = true;
        }
    }
}
