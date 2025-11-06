using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using HeimdallApi.Examples; 
using Swashbuckle.AspNetCore.Filters;
using System.Threading.Tasks; 
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace UsuariosApi.Controllers 
{

    [ApiController]
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")] 
    [Authorize]

    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService usuarioService;
        private readonly LinkGenerator linkGenerator;

        public UsuarioController(UsuarioService usuarioService, LinkGenerator linkGenerator)
        {
            this.usuarioService = usuarioService;
            this.linkGenerator = linkGenerator;
        }

        private object GetUsuarioResource(UsuarioModel usuario)
        {
            return new
            {
                usuario.id,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.DataNascimento,
                usuario.Cpf,
                usuario.Email,
                Categoria = usuario.CategoriaUsuario != null ? new 
                {
                    usuario.CategoriaUsuario.Id,
                    usuario.CategoriaUsuario.Nome
                } : null,
                links = new
                {
                    self = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { version = "1.0", id = usuario.id }),
                    update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Usuario", new { version = "1.0", id = usuario.id }),
                    delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Usuario", new { version = "1.0", id = usuario.id }),
                    all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new{ version = "1.0"})
                }
            };
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(Summary = "Obtém todos os usuários", Description = "Retorna uma lista de todos os usuários cadastrados com paginação.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UsuarioListResponseExample))]

        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = usuarioService.ListarUsuario();

            var totalItems = await query.CountAsync();
            if (totalItems == 0)
                return NoContent();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var usuariosPage = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var usuariosComLinks = usuariosPage.Select(u => GetUsuarioResource(u)).ToList();

            var result = new PagedResultDto<object>
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Links = new
                {
                    self = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { page, pageSize }),
                    next = page < totalPages ? linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { page = page + 1, pageSize }) : null,
                    prev = page > 1 ? linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { page = page - 1, pageSize }) : null
                },
                Items = usuariosComLinks
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Obtém um usuário pelo ID", Description = "Retorna os dados de um usuário específico pelo ID.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UsuarioExample))]
        public IActionResult Get(int id)
        {
            var usuario = usuarioService.ObterPorId(id);
            return usuario == null ? NotFound() : Ok(GetUsuarioResource(usuario));
        }

[HttpGet("nome")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[SwaggerOperation(Summary = "Obtém usuários pelo nome", Description = "Retorna uma lista de usuários filtrados pelo nome.")]
public async Task<IActionResult> GetPorNome([FromQuery] string nome)
{

    var usuarios = await usuarioService.ListarUsuario()
        .Where(u => u.Nome.ToLower().Contains(nome.ToLower())) 
        .ToListAsync();
    
    if (usuarios == null || !usuarios.Any()) 
    {
        return NotFound("Nenhum usuário encontrado para este nome.");
    }
    
    var resultado = usuarios.Select(u => GetUsuarioResource(u)).ToList();
    return Ok(resultado);
}

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Cadastra um novo usuário", Description = "Cadastra um novo usuário no sistema.")]
        [SwaggerRequestExample(typeof(UsuarioModel), typeof(UsuarioExample))]
        public IActionResult Post([FromBody] UsuarioModel usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nome) ||
                string.IsNullOrWhiteSpace(usuario.Sobrenome) ||
                string.IsNullOrWhiteSpace(usuario.DataNascimento) ||
                string.IsNullOrWhiteSpace(usuario.Cpf) ||
                string.IsNullOrWhiteSpace(usuario.Email) ||
                string.IsNullOrWhiteSpace(usuario.Senha) ||
                usuario.CategoriaUsuarioId <= 0) 
            {
                return BadRequest("Todos os campos são obrigatórios e a CategoriaUsuarioId deve ser válida.");
            }

            var criada = usuarioService.CadastrarUsuario(usuario);

            if (criada == null)
            {
                return BadRequest("Email já existente ou Categoria de Usuário inválida.");
            }

            var resource = GetUsuarioResource(criada);
            return CreatedAtAction(nameof(Get), new { id = criada.id }, resource);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Atualiza os dados de um usuário", Description = "Atualiza os dados de um usuário existente.")]
        [SwaggerRequestExample(typeof(UsuarioModel), typeof(UsuarioExample))]
        public IActionResult Put(int id, [FromBody] UsuarioModel usuario)
        {
            if (usuario == null || usuario.id != id)
                return BadRequest("Dados inconsistentes.");
                
            // 1. Verifica se o usuário existe
            var existente = usuarioService.ObterPorId(id);
            if (existente == null)
            {
                return NotFound(); 
            }

            // 2. Tenta atualizar
            var sucesso = usuarioService.AtualizarUsuario(usuario);

            //Se falhar, é por erro de re regra de negócio
            if (!sucesso)
            {
                return BadRequest("Email já existente ou Categoria de Usuário inválida.");
            }

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Remove um usuário", Description = "Remove um usuário do sistema com base no ID.")]
        public IActionResult Delete(int id)
        {
            return usuarioService.RemoverUsuario(id) ? NoContent() : NotFound();
        }
    }
}
