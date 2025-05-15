using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using MetroShip.Utility.Config;
using Microsoft.IdentityModel.Tokens;

namespace MetroShip.Service.Utils
{
    public class JwtUtils
    {
        public static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SystemSettingModel.Instance.SecretKey))
            };
        }

        public static string GenerateToken(IEnumerable<Claim> claims, int hour)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SystemSettingModel.Instance.SecretKey);
            var credential = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.Now,
                Expires = DateTime.Now.AddHours(hour),
                SigningCredentials = credential
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static bool ValidateToken(string authToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetTokenValidationParameters();

            try
            {
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out var validatedToken);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
