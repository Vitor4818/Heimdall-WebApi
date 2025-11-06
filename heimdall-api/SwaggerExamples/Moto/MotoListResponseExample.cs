using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace MotosApi.SwaggerExamples
{
    public class MotoListResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                page = 1,
                pageSize = 10,
                totalPages = 2,
                totalItems = 12,
                links = new
                {
                    self = "/api/v1/Motos?page=1&pageSize=10",
                    next = "/api/v1/Motos?page=2&pageSize=10",
                    prev = (string?)null
                },
                items = new[]
                {
                    new
                    {
                        id = 1,
                        tipoMoto = "Esportiva",
                        placa = "ABC1234",
                        numChassi = "9C2JC4110JR000001",
                        kmRodados = 1000.0f,
                        vagaId = 1,
                        vaga = new
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
                    }
                }
            };
        }
    }
}
