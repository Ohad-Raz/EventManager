using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManager.WebAPI.Security
{
    public class JwtTokenProvider
    {
        /// <summary>
        /// Creates JWT token for authenticated user.
        /// Can include username and role claims.
        /// </summary>
        public static string CreateToken(string secureKey, int expiration, string? subject = null, string? role = null)
        {
            // Convert secret key to byte array
            byte[] tokenKey = Encoding.UTF8.GetBytes(secureKey);

            // Prepare claims that will be stored inside token
            List<Claim> claims = new List<Claim>();

            // Add username-related claims
            if (!string.IsNullOrEmpty(subject))
            {
                claims.Add(new Claim(ClaimTypes.Name, subject));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
            }

            // Add role claim for role-based authorization
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Token descriptor = token "template"
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiration),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Create token and serialize it to string
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }
    }
}