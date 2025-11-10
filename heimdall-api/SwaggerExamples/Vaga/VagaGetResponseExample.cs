using Swashbuckle.AspNetCore.Filters;
using HeimdallModel; // Ajuste o namespace

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload de resposta ao buscar detalhes de uma Vaga por ID.
    /// </summary>
    public class VagaGetResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                Id = 101,
                Codigo = "ZC1-VG101",
                Ocupada = true, 
                ZonaId = 1,
                
                Zona = new
                {
                    Id = 1,
                    Nome = "ZC1-COMBUSTÃO",
                    Tipo = "Combustao"
                },

                Moto = new
                {
                    Id = 26,
                    Placa = "ABC1234",
                    TipoMoto = "Esportiva",
                    links = new
                    {
                        self = "/api/v1/Moto/26"
                    }
                },
                
                links = new
                {
                    self = "/api/v1/Vaga/101",
                    update = "/api/v1/Vaga/101",
                    delete = "/api/v1/Vaga/101"
                }
            };
        }
    }
}