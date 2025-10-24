using HeimdallBusiness;
using HeimdallModel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // Classe DTO simples dentro do controller
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _authService.Authenticate(request.Email, request.Password);
        if (user == null)
            return BadRequest(new { message = "Usuário ou senha inválidos" });

        var token = _authService.GenerateToken(user);

        return Ok(new { token, user });
    }
}
