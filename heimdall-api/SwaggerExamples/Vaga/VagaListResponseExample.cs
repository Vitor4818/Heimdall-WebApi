using Swashbuckle.AspNetCore.Filters;
using HeimdallModel; 

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload de resposta ao listar todas as Vagas.
    /// </summary>
    public class VagaListResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                page = 1,
                pageSize = 10,
                totalPages = 1,
                totalItems = 5,
                Items = new[]
                {
                    new
                    {
                        Id = 101,
                        Codigo = "ZC1-VG101",
                        Ocupada = true,
                        ZonaId = 1
                    },
                    new
                    {
                        Id = 102,
                        Codigo = "ZC1-VG102",
                        Ocupada = false,
                        ZonaId = 1
                    }
                },
                links = new
                {
                    self = "/api/v1/Vaga?page=1&pageSize=10"
                }
            };
        }
    }
}