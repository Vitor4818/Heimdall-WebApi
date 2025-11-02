using HeimdallBusiness;
using HeimdallModel;
using HeimdallModel.DTOs;

using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;

namespace HeimdallApi.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly TokenService _tokenService;

        public AuthController(UsuarioService usuarioService, TokenService tokenService)
        {
            _usuarioService = usuarioService;
            _tokenService = tokenService;
        }


        [HttpPost("registrar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Registrar([FromBody] RegistroDto registroDto)
        {
            var novoUsuario = new UsuarioModel
            {
                id = registroDto.Id, 
                Nome = registroDto.Nome,
                Sobrenome = registroDto.Sobrenome,
                DataNascimento = registroDto.DataNascimento,
                Cpf = registroDto.Cpf,
                Email = registroDto.Email,
                Senha = registroDto.Senha,
                CategoriaUsuarioId = registroDto.CategoriaUsuarioId
            };

            var usuarioCriado = _usuarioService.CadastrarUsuario(novoUsuario);

            if (usuarioCriado == null)
            {
                return BadRequest("Email já existente ou Categoria de Usuário inválida.");
            }

            return CreatedAtAction(nameof(Login), new { Email = usuarioCriado.Email }, new { message = "Usuário registrado com sucesso." });
        }



        [HttpPost("login")]
        [AllowAnonymous] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            // 1. O UsuarioService (corrigido) agora verifica o hash do BCrypt
            var usuario = _usuarioService.Login(loginDto.Email, loginDto.Senha);

            if (usuario == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // 2. O TokenService gera o token
            var tokenString = _tokenService.GenerateToken(usuario);

            // 3. Retorna o token para o cliente
            return Ok(new
            {
                message = "Login bem-sucedido.",
                token = tokenString,
                usuario = new 
                {
                    usuario.id,
                    usuario.Nome,
                    usuario.Email,
                    Categoria = usuario.CategoriaUsuario?.Nome
                }
            });
        }
    }
}
