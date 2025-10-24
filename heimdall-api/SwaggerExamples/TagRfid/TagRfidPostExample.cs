using Swashbuckle.AspNetCore.Filters;
using HeimdallModel;

namespace HeimdallApi.SwaggerExamples.TagRfid
{
    public class TagRfidPostExample : IExamplesProvider<TagRfidModel>
    {
        public TagRfidModel GetExamples()
        {
            return new TagRfidModel
            {
                Id = 1,
                MotoId = 1,
                FaixaFrequencia = "865-868 MHz",
                Banda = "UHF",
                Aplicacao = "Rastreamento de frota"
            };
        }
    }
}
