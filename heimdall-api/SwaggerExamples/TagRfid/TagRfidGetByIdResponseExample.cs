using Swashbuckle.AspNetCore.Filters;

namespace HeimdallApi.SwaggerExamples.TagRfid
{
    public class TagRfidGetByIdResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                id = 1,
                motoId = 1,
                faixaFrequencia = "865-868 MHz",
                banda = "UHF",
                aplicacao = "Rastreamento de frota",
                links = new
                {
                    // --- CORRIGIDO (VERSIONAMENTO) ---
                    self = "/api/v1/TagRfid/1",
                    update = "/api/v1/TagRfid/1",
                    delete = "/api/v1/TagRfid/1",
                    all = "/api/v1/TagRfid"
                    // --- FIM DA CORREÇÃO ---
                }
            };
        }
    }
}
