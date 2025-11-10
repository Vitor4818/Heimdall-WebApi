using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload de resposta ao listar todas as Zonas.
    /// </summary>
    public class ZonaListResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                page = 1,
                pageSize = 10,
                totalPages = 1,
                totalItems = 3,
                items = new[]
                {
                    new
                    {
                        Id = 1,
                        Nome = "ZC1-COMBUSTÃO",
                        Tipo = "Combustao"
                    },
                    new
                    {
                        Id = 2,
                        Nome = "ZE1-ELÉTRICA",
                        Tipo = "Elétrica"
                    }
                },
                links = new
                {
                    self = "/api/v1/Zona?page=1&pageSize=10"
                }
            };
        }
    }
}