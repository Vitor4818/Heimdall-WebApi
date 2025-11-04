using HeimdallModel;
using Swashbuckle.AspNetCore.Filters;

namespace MotosApi.SwaggerExamples
{
    /// <summary>
    /// Exemplo de payload para POST (Cadastrar) ou PUT (Atualizar) uma Moto.
    /// </summary>
    public class MotoExample : IExamplesProvider<MotoModel>
    {
        public MotoModel GetExamples()
        {
            return new MotoModel
            {
                id = 1,
                tipoMoto = "Esportiva",
                placa = "ABC1234",
                numChassi = "9C2JC4110JR000001",
                KmRodados = 1000, 
                VagaId = null 
            };
        }
    }
}
