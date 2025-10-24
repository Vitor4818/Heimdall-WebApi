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
                    self = "/api/TagRfid/1",
                    update = "/api/TagRfid/1",
                    delete = "/api/TagRfid/1",
                    all = "/api/TagRfid"
                }
            };
        }
    }
}
