using Microsoft.ML.Data; 

namespace HeimdallModel.DTOs
{

    public class MotoRevisaoData
    {
        [LoadColumn(0)] 
        public float KmRodados { get; set; }
        
        [LoadColumn(1)]
        [ColumnName("Label")] 
        public bool PrecisaRevisaoLabel { get; set; }
    }
}

