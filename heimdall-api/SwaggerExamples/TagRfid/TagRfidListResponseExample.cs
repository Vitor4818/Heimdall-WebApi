using Swashbuckle.AspNetCore.Filters;

namespace HeimdallApi.SwaggerExamples.TagRfid
{
    public class TagRfidListResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                page = 1,
                pageSize = 10,
                totalPages = 2,
                totalItems = 15,
                links = new
                {
                    self = "/api/TagRfid?page=1&pageSize=10",
                    next = "/api/TagRfid?page=2&pageSize=10",
                    prev = (string?)null
                },
                items = new[]
                {
                    new
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
                    }
                }
            };
        }
    }
}
