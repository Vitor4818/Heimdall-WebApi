namespace HeimdallModel;

/// <summary>
/// Representa uma zona do pátio.
/// </summary>
public class ZonaModel
{
    /// <summary>
    /// Identificador único da zona.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Nome da zona (ex: ZC1, ZC2, ZE).
    /// </summary>
    public required string Nome { get; set; }

    /// <summary>
    /// Tipo da zona (ex: Combustão, Elétrica).
    /// </summary>
    public required string Tipo { get; set; }

    /// <summary>
    /// Lista de vagas pertencentes à zona.
    /// </summary>
    public ICollection<VagaModel>? Vagas { get; set; }
}
