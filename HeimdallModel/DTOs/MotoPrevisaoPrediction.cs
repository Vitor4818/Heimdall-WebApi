using Microsoft.ML.Data;

namespace HeimdallModel.DTOs
{

    public class MotoRevisaoPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PrecisaRevisao { get; set; }

        public float Probability { get; set; }
        public float Score { get; set; }
    }

    public class AnaliseRevisaoDto
    {
        public required float KmRodados { get; set; }
    }
}
