using System.ComponentModel.DataAnnotations;

namespace AuditoriaNFe.Application.Configuracao;

/// <summary>Timeouts obrigatórios dos bancos e aplicação de migrações (constitution 5.4; research R-16).</summary>
public sealed class PersistenciaOpcoes
{
    public const string Secao = "Persistencia";

    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "00:01:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan? TimeoutMongo { get; set; }

    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "00:01:00", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public TimeSpan? TimeoutPostgres { get; set; }

    /// <summary><c>true</c> só no ambiente local.</summary>
    public bool AplicarMigracoes { get; set; }
}
