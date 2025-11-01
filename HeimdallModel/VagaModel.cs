namespace HeimdallModel;

/// <summary>
/// Representa uma vaga de estacionamento de motocicleta.
/// </summary>
public class VagaModel
{
    /// <summary>
    /// Identificador único da vaga.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Código da vaga (ex: ZC1VG1).
    /// </summary>
    public required string Codigo { get; set; }

    /// <summary>
    /// Indica se a vaga está ocupada.
    /// </summary>
    public bool Ocupada { get; set; }

    /// <summary>
    /// Identificador da zona à qual a vaga pertence.
    /// </summary>
    public int ZonaId { get; set; }

    /// <summary>
    /// Zona à qual a vaga pertence.
    /// </summary>
    public ZonaModel? Zona { get; set; }

    /// <summary>
    /// Moto estacionada na vaga (se houver).
    /// </summary>
    public MotoModel? Moto { get; set; }
}
