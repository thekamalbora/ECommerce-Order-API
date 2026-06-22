using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(
            IConfiguration config)
        {
            _config = config;
        }

        public string Generate(
            User user)
        {
            var claims =
                new[]
                {
                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
                };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _config["Jwt:Key"]));

            var creds =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer:
                    _config["Jwt:Issuer"],

                    audience:
                    _config["Jwt:Audience"],

                    claims:

                    claims,

                    expires:
                    DateTime.Now.AddHours(2),

                    signingCredentials:
                    creds);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
