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
                    self = "/api/Motos?page=1&pageSize=10",
                    next = "/api/Motos?page=2&pageSize=10",
                },
                items = new[]
                {
                    new
                    {
                        id = 1,
                        tipoMoto = "Esportiva",
                        placa = "ABC1234",
                        numChassi = "9C2JC4110JR000001",
                        tagRfid = new
                        {
                            Id = 1,
                            MotoId = 1,
                            FaixaFrequencia = "865-868 MHz",
                            Banda = "UHF",
                            Aplicacao = "Rastreamento"
                        },
                        links = new
                        {
                            self = "/api/Motos/1",
                            update = "/api/Motos/1",
                            delete = "/api/Motos/1",
                            all = "/api/Motos"
                        }
                    }
                }
            };
        }
    }
}
