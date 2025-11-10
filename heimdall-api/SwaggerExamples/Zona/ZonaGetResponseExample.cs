using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload de resposta ao buscar detalhes de uma Zona por ID.
    /// </summary>
    public class ZonaGetResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                Id = 1,
                Nome = "ZC1-COMBUSTÃO",
                Tipo = "Combustao",
                
                Vagas = new[] 
                {
                    new 
                    {
                        Id = 101,
                        Codigo = "VGC101",
                        Ocupada = false
                    },
                    new
                    {
                        Id = 102,
                        Codigo = "VGC102",
                        Ocupada = true
                    }
                },

                links = new
                {
                    self = "/api/v1/Zona/1",
                    update = "/api/v1/Zona/1",
                    delete = "/api/v1/Zona/1"
                }
            };
        }
    }
}