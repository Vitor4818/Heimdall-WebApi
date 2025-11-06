using Swashbuckle.AspNetCore.Filters;

namespace HeimdallApi.Examples
{
    public class UsuarioListResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                page = 1,
                pageSize = 10,
                totalPages = 1,
                totalItems = 2,
                links = new
                {
                    self = "/api/usuario?page=1&pageSize=10",
                    next = (string?)null,
                    prev = (string?)null
                },
                items = new[]
                {
                    new {
                        id = 1,
                        nome = "Vitor",
                        sobrenome = "Gomes",
                        dataNascimento = "10/04/2006",
                        cpf = "123.456.789-00",
                        email = "vitor.gomes@exemplo.com",
                        links = new {
                           self = "/api/v1/Usuario/1",
                            update = "/api/v1/Usuario/1",
                            delete = "/api/v1/Usuario/1",
                            all = "/api/v1/Usuario"
                        }
                    },
                    new {
                        id = 2,
                        nome = "Lucas",
                        sobrenome = "Mendes",
                        dataNascimento = "22/07/2005",
                        cpf = "987.654.321-00",
                        email = "lucas.mendes@exemplo.com",
                        links = new {
                            self = "/api/v1/Usuario/2",
                            update = "/api/v1/Usuario/2",
                            delete = "/api/v1/Usuario/2",
                            all = "/api/v1/Usuario"
                        }
                    }
                }
            };
        }
    }
}
