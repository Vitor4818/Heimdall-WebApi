using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace HeimdallApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload para POST (Cadastrar) ou PUT (Atualizar) uma Zona.
    /// </summary>
    public class ZonaExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                Id = 1,
                Nome = "ZC1-COMBUSTÃO",
                Tipo = "Combustao"
            };
        }
    }
}