using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Tourist.API.Models;

namespace Tourist.API.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly Dictionary<string, string> _refreshTokens = new();
        public readonly IConfiguration _configuration;
        public TokenRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> CreateJwtToken(IdentityUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.Email,user.Email)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );

            var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
            return await Task.FromResult(jwttoken);
        }

        public Task<string> GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Task.FromResult(Convert.ToBase64String(randomBytes));
        }

        public Task SaveRefreshToken(ApplicationUser user, string refreshToken)
        {
            _refreshTokens[user.Email] = refreshToken;
            return Task.CompletedTask;
        }

        public Task<bool> ValidateRefreshToken(ApplicationUser user, string refreshToken)
        {
            return Task.FromResult(_refreshTokens.TryGetValue(user.Email, out var stored) && stored == refreshToken);
        }
    }
}
