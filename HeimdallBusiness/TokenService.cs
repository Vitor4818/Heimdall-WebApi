using HeimdallModel;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text; 

namespace HeimdallBusiness
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UsuarioModel usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            //Pega a chave secreta configurada no AppSettings
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            //As "Claims" (informações) que queremos guardar dentro do token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome), 
                new Claim(ClaimTypes.Email, usuario.Email),
            };

            //Adiciona a "Role" (Categoria) do usuário às claims
            if (usuario.CategoriaUsuario != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.CategoriaUsuario.Nome));
            }

            //Configuração do Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                
                //Emissor e Audiência (lidos do appsettings.json)
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                
                //Tempo de expiração (ex: 2 horas)
                Expires = DateTime.UtcNow.AddHours(2),

                //Credenciais de assinatura (usando a chave secreta)
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
