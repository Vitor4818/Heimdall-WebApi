using HeimdallModel;
using Swashbuckle.AspNetCore.Filters;

namespace MotosApi.SwaggerExamples
{
    public class MotoExample : IExamplesProvider<MotoModel>
    {
        public MotoModel GetExamples()
        {
            return new MotoModel
            {
                id = 1,
                tipoMoto = "Esportiva",
                placa = "ABC1234",
                numChassi = "9C2JC4110JR000001",
                TagRfid = new TagRfidModel
                {
                    Id = 1,
                    FaixaFrequencia = "125 kHz",
                    Banda = "LF",
                    Aplicacao = "Controle de acesso",
                    MotoId = 1
                }
            };
        }
    }
}
