using JNJServices.Utility.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JNJServices.Utility.Security
{
    public class JwtFactory : IJwtFactory
    {
        private readonly IConfiguration _configuration;
        private readonly TimeZoneConverter _timeZoneConverter;
        public JwtFactory(IConfiguration configuration, TimeZoneConverter timeZoneConverter)
        {
            _configuration = configuration;
            _timeZoneConverter = timeZoneConverter;
        }

        public string GenerateToken(List<Claim> claims)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = _timeZoneConverter.ConvertUtcToConfiguredTimeZone().AddDays(365),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
