using HeimdallModel;
using Swashbuckle.AspNetCore.Filters;

namespace HeimdallApi.Examples
{
    public class UsuarioExample : IExamplesProvider<UsuarioModel>
    {
        public UsuarioModel GetExamples()
        {
            return new UsuarioModel
            {
                id = 1,
                CategoriaUsuarioId = 2,
                Nome = "Vitor",
                Sobrenome = "Gomes",
                DataNascimento = "10/04/2006",
                Cpf = "123.456.789-00",
                Email = "vitor.gomes@exemplo.com",
                Senha = "SenhaForte123"
            };
        }
    }
}
