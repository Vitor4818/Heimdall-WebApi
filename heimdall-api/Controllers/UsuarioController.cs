using Microsoft.AspNetCore.Mvc; 
using HeimdallModel;   
using HeimdallBusiness;
namespace UsuariosApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UsuarioController(UsuarioService usuarioService): ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var usuarios = usuarioService.ListarUsuario();
        return usuarios.Count == 0 ? NoContent() : Ok(usuarios);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var usuarios = usuarioService.ObterPorId(id);
        return usuarios == null ? NotFound() : Ok(usuarios);
    }

    [HttpGet("nome")]
    public IActionResult GetPorNome(string nome)
    {
        var usuario = usuarioService.ObterPorNome(nome);
        return usuario == null ? NotFound() : Ok(usuario);
    }


    [HttpPost]
    public IActionResult Post([FromBody] UsuarioModel usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nome) ||
    string.IsNullOrWhiteSpace(usuario.Sobrenome) ||
    string.IsNullOrWhiteSpace(usuario.DataNascimento) ||
    string.IsNullOrWhiteSpace(usuario.Cpf) ||
    string.IsNullOrWhiteSpace(usuario.Email) ||
    string.IsNullOrWhiteSpace(usuario.Senha))
    return BadRequest("Tipo da moto e placa são obrigatórios.");
var criada = usuarioService.CadastrarUsuario(usuario);
return CreatedAtAction(nameof(Get), new {id = criada.id}, criada);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] UsuarioModel usuario)
    {
        if(usuario == null || usuario.id != id)
        return BadRequest ("Dados Inconsistentes");
        
return usuarioService.AtualizarUsuario(usuario) ? NoContent() : NotFound();    }
    
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return usuarioService.RemoverUsuario(id) ? NoContent(): NotFound();
    }


}

