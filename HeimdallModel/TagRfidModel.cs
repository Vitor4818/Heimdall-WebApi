namespace HeimdallModel
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Representa uma tag RFID utilizada para identificação de uma moto no sistema.
    /// </summary>
    public class TagRfidModel
    {
        /// <summary>
        /// Identificador único da tag RFID.
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Identificador da moto associada a esta tag.
        /// </summary>
        public required int MotoId { get; set; }

        /// <summary>
        /// Faixa de frequência utilizada pela tag RFID (exemplo: 860-960 MHz).
        /// </summary>
        public required string FaixaFrequencia { get; set; }

        /// <summary>
        /// Banda de operação da tag RFID (exemplo: UHF, HF, LF).
        /// </summary>
        public required string Banda { get; set; }

        /// <summary>
        /// Aplicação ou uso da tag RFID (exemplo: controle de acesso, rastreamento de veículos, etc.).
        /// </summary>
        public required string Aplicacao { get; set; }

        /// <summary>
        /// Moto associada à tag RFID. Ignorada durante a serialização JSON.
        /// </summary>
        [JsonIgnore]
        public MotoModel? Moto { get; set; }
    }
}
