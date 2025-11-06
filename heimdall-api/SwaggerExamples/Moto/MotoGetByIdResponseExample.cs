using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace MotosApi.SwaggerExamples
{
    public class MotoGetByIdResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                id = 1,
                tipoMoto = "Esportiva",
                placa = "ABC1234",
                numChassi = "9C2JC4110JR000001",
                kmRodados = 1000.0f, // <-- ADICIONADO
                vagaId = 1, // <-- ADICIONADO
                vaga = new // <-- ADICIONADO
                {
                    Id = 1,
                    Codigo = "V1",
                    Ocupada = true,
                    ZonaId = 1,
                    links = new
                    {
                        self = "/api/v1/Vaga/1"
                    }
                },
                tagRfid = new
                {
                    Id = 1,
                    MotoId = 1,
                    FaixaFrequencia = "865-868 MHz",
                    Banda = "UHF",
                    Aplicacao = "Rastreamento",
                    links = new 
                    {
                        self = "/api/v1/TagRfid/1"
                    }
                },
                links = new
                {
                    self = "/api/v1/Motos/1",
                    update = "/api/v1/Motos/1",
                    delete = "/api/v1/Motos/1",
                    all = "/api/v1/Motos"
                }
            };
        }
    }
}
