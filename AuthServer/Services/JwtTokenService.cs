using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthServer.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<IdentityUser> userManager;

        public JwtTokenService(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(IdentityUser user)
        {
            var jwtKey = configuration["Jwt:Key"] ?? "Lab789SuperSecretKeyForJwtAuthentication2026!@#$";
            var issuer = configuration["Jwt:Issuer"] ?? "Lab789AuthServer";
            var audience = configuration["Jwt:Audience"] ?? "Lab789Clients";
            var expireMinutes = int.TryParse(configuration["Jwt:ExpireMinutes"], out var minutes) ? minutes : 120;

            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
