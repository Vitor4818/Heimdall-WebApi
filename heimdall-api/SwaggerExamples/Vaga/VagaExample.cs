using Swashbuckle.AspNetCore.Filters;
using HeimdallModel; 

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload para POST (Cadastrar) ou PUT (Atualizar) uma Vaga.
    /// </summary>
    public class VagaExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                Id = 1,
                Codigo = "ZC1-VG101",
                ZonaId = 1
            };
        }
    }
}