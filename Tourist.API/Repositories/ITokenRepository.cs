using Microsoft.AspNetCore.Identity;
using Tourist.API.Models;

namespace Tourist.API.Repositories
{
    public interface ITokenRepository
    {
        Task<string> CreateJwtToken(IdentityUser user, List<string> roles);
        Task<string> GenerateRefreshToken();       
    }
}
