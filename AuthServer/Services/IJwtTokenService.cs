using Microsoft.AspNetCore.Identity;

namespace AuthServer.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(IdentityUser user);
    }
}
