using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourist.API.Models;
using Tourist.API.Models.Dto;
using Tourist.API.Repositories;

namespace Tourist.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenRepository;
        public AuthController(UserManager<ApplicationUser> userManager, IMapper mapper, ITokenRepository tokenRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenRepository = tokenRepository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var user = _mapper.Map<ApplicationUser>(request);

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Reader");
                return Ok("User registered successfully");
            }
            else
            {
                return BadRequest(identityResult.Errors);
            }

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is not null)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(identityUser, request.Password);
                if (checkPasswordResult)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var jwtToken = await _tokenRepository.CreateJwtToken(identityUser, roles.ToList());
                    var response = new LoginResponseDto { Email = request.Email, Roles = roles.ToList(), Token = jwtToken };
                    return Ok(response);
                }
            }
            return Unauthorized("Invalid Credentials");
        }
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok("API is running");
        }
    }
}
