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

    public UsuarioController(UsuarioService usuarioService)
    {
        this.usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtém todos os usuários cadastrados.
    /// </summary>
    /// <returns>Lista de usuários.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerOperation(Summary = "Obtém todos os usuários", Description = "Retorna uma lista de todos os usuários cadastrados.")]
    public IActionResult Get()
    {
        var usuarios = usuarioService.ListarUsuario();
        return usuarios.Count == 0 ? NoContent() : Ok(usuarios);
    }

    /// <summary>
    /// Obtém um usuário específico pelo ID.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <returns>Usuário com o ID fornecido.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém um usuário pelo ID", Description = "Retorna os dados de um usuário específico pelo ID.")]
    public IActionResult Get(int id)
    {
        var usuario = usuarioService.ObterPorId(id);
        return usuario == null ? NotFound() : Ok(usuario);
    }

    /// <summary>
    /// Obtém um usuário específico pelo nome.
    /// </summary>
    /// <param name="nome">Nome do usuário.</param>
    /// <returns>Usuário com o nome fornecido.</returns>
    [HttpGet("nome")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém um usuário pelo nome", Description = "Retorna os dados de um usuário específico pelo nome.")]
    public IActionResult GetPorNome(string nome)
    {
        var usuario = usuarioService.ObterPorNome(nome);
        return usuario == null ? NotFound() : Ok(usuario);
    }

    /// <summary>
    /// Cadastra um novo usuário.
    /// </summary>
    /// <param name="usuario">Dados do usuário a ser cadastrado.</param>
    /// <returns>Usuário cadastrado.</returns>
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
        return CreatedAtAction(nameof(Get), new { id = criada.id }, criada);
    }

    /// <summary>
    /// Atualiza os dados de um usuário existente.
    /// </summary>
    /// <param name="id">ID do usuário a ser atualizado.</param>
    /// <param name="usuario">Dados atualizados do usuário.</param>
    /// <returns>Resposta de sucesso ou falha.</returns>
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

    /// <summary>
    /// Remove um usuário do sistema.
    /// </summary>
    /// <param name="id">ID do usuário a ser removido.</param>
    /// <returns>Resposta de sucesso ou falha.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Remove um usuário", Description = "Remove um usuário do sistema com base no ID.")]
    public IActionResult Delete(int id)
    {
        return usuarioService.RemoverUsuario(id) ? NoContent() : NotFound();
    }
}
