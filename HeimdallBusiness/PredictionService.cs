using Microsoft.ML;
using HeimdallModel.DTOs;
using System.Collections.Generic;

namespace HeimdallBusiness
{
    public class PredictionService
    {
        private readonly PredictionEngine<MotoRevisaoData, MotoRevisaoPrediction> _engine;

        public PredictionService()
        {
            //INICIA o ML.NET
            var mlContext = new MLContext();

            //2. CRIA OS DADOS DE TREINO 
            var trainingData = new[]
            {
                new MotoRevisaoData { KmRodados = 1000, PrecisaRevisaoLabel = false },
                new MotoRevisaoData { KmRodados = 3000, PrecisaRevisaoLabel = false },
                new MotoRevisaoData { KmRodados = 5000, PrecisaRevisaoLabel = false },
                new MotoRevisaoData { KmRodados = 7000, PrecisaRevisaoLabel = false },
                new MotoRevisaoData { KmRodados = 9000, PrecisaRevisaoLabel = true }, 
                new MotoRevisaoData { KmRodados = 10000, PrecisaRevisaoLabel = true },
                new MotoRevisaoData { KmRodados = 12000, PrecisaRevisaoLabel = true },
                new MotoRevisaoData { KmRodados = 15000, PrecisaRevisaoLabel = true }
            };

            var trainingDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            //3. CRIA O PIPELINE
            var pipeline = mlContext.Transforms
                .Concatenate("Features", nameof(MotoRevisaoData.KmRodados))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            // 4. TREINA O MODELO 
            var model = pipeline.Fit(trainingDataView);

            // 5. CRIA O "MOTOR" DE PREVISÃO
            _engine = mlContext.Model.CreatePredictionEngine<MotoRevisaoData, MotoRevisaoPrediction>(model);
        }

        public MotoRevisaoPrediction PreverRevisao(float kmRodados)
        {
            var input = new MotoRevisaoData { KmRodados = kmRodados }; 
            return _engine.Predict(input);
        }
    }
}

