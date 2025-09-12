using Microsoft.AspNetCore.Identity;

namespace Tourist.API.Repositories
{
    public interface ITokenRepository
    {
        Task<string> CreateJwtToken(IdentityUser user,List<string> roles);
    }
}
