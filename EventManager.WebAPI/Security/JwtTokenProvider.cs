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
        /// Can include username, user id, and role claims.
        /// </summary>
        public static string CreateToken(string secureKey, int expiration, string? subject = null, string? role = null, int? userId = null)
        {
            // Convert secret key to byte array
            byte[] tokenKey = Encoding.UTF8.GetBytes(secureKey);

            // Prepare claims that will be stored inside token
            List<Claim> claims = new List<Claim>();

            // Add username-related claims
            if (!string.IsNullOrEmpty(subject))
            {
                // ClaimTypes.Name is written to the JWT as "unique_name"
                claims.Add(new Claim(ClaimTypes.Name, subject));
                // Standard JWT subject claim, written as "sub"
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
            }

            if (userId.HasValue)
            {
                // ClaimTypes.NameIdentifier is written to the JWT as "nameid"
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            // Add role claim for role-based authorization
            if (!string.IsNullOrEmpty(role))
            {
                // ClaimTypes.Role is written to the JWT as "role"
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