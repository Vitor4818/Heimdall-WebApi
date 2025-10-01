using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;

namespace UsuariosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService usuarioService;
    private readonly LinkGenerator linkGenerator;

    public UsuarioController(UsuarioService usuarioService, LinkGenerator linkGenerator)
    {
        this.usuarioService = usuarioService;
        this.linkGenerator = linkGenerator;
    }

    // Método auxiliar para montar os links HATEOAS
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
            links = new
            {
                self = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { id = usuario.id }),
                update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Usuario", new { id = usuario.id }),
                delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Usuario", new { id = usuario.id }),
                all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario")
            }
        };
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerOperation(Summary = "Obtém todos os usuários", Description = "Retorna uma lista de todos os usuários cadastrados.")]
public IActionResult Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 10;

    var allUsuarios = usuarioService.ListarUsuario();
    if (!allUsuarios.Any()) return NoContent();

    var totalItems = allUsuarios.Count;
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var usuariosPage = allUsuarios
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new
        {
            u.id,
            u.Nome,
            u.Sobrenome,
            u.DataNascimento,
            u.Cpf,
            u.Email,
            links = new
            {
                self = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario", new { id = u.id }),
                update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Usuario", new { id = u.id }),
                delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Usuario", new { id = u.id }),
                all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Usuario")
            }
        })
        .Cast<object>()
        .ToList();

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
        Items = usuariosPage
    };

    return Ok(result);
}


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém um usuário pelo ID", Description = "Retorna os dados de um usuário específico pelo ID.")]
    public IActionResult Get(int id)
    {
        var usuario = usuarioService.ObterPorId(id);
        return usuario == null ? NotFound() : Ok(GetUsuarioResource(usuario));
    }

    [HttpGet("nome")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém um usuário pelo nome", Description = "Retorna os dados de um usuário específico pelo nome.")]
    public IActionResult GetPorNome(string nome)
    {
        var usuario = usuarioService.ObterPorNome(nome);
        return usuario == null ? NotFound() : Ok(GetUsuarioResource(usuario));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Cadastra um novo usuário", Description = "Cadastra um novo usuário no sistema.")]
    public IActionResult Post([FromBody] UsuarioModel usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nome) ||
            string.IsNullOrWhiteSpace(usuario.Sobrenome) ||
            string.IsNullOrWhiteSpace(usuario.DataNascimento) ||
            string.IsNullOrWhiteSpace(usuario.Cpf) ||
            string.IsNullOrWhiteSpace(usuario.Email) ||
            string.IsNullOrWhiteSpace(usuario.Senha))
        {
            return BadRequest("Todos os campos são obrigatórios.");
        }

        var criada = usuarioService.CadastrarUsuario(usuario);

        var resource = GetUsuarioResource(criada);
        return CreatedAtAction(nameof(Get), new { id = criada.id }, resource);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Atualiza os dados de um usuário", Description = "Atualiza os dados de um usuário existente.")]
    public IActionResult Put(int id, [FromBody] UsuarioModel usuario)
    {
        if (usuario == null || usuario.id != id)
            return BadRequest("Dados inconsistentes.");

        return usuarioService.AtualizarUsuario(usuario) ? NoContent() : NotFound();
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
