namespace HeimdallModel
{
    /// <summary>
    /// Representa uma motocicleta cadastrada no sistema.
    /// </summary>
    public class MotoModel
    {
        /// <summary>
        /// Identificador único da moto.
        /// </summary>
        public required int id { get; set; }

        /// <summary>
        /// Tipo da moto (exemplo: Esportiva, Custom, Street, etc.).
        /// </summary>
        public required string tipoMoto { get; set; }

        /// <summary>
        /// Placa da moto.
        /// </summary>
        public required string placa { get; set; }

        /// <summary>
        /// Número do chassi da moto.
        /// </summary>
        public required string numChassi { get; set; }

        /// <summary>
        /// Tag RFID associada à moto, usada para identificação.
        /// </summary>
        public TagRfidModel? TagRfid { get; set; }

        /// <summary>
        /// Quilometragem total da moto (usada para prever revisão).
        /// </summary>
        public float KmRodados { get; set; } = 0;

         /// <summary>
        /// Identificador da vaga onde a moto está estacionada (se houver).
        /// </summary>
        public int? VagaId { get; set; }

        /// <summary>
        /// Vaga onde a moto está estacionada.
        /// </summary>
        public VagaModel? Vaga { get; set; }
    }
}
