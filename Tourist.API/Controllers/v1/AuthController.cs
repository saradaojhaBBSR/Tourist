using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourist.API.Models;
using Tourist.API.Models.Dto;
using Tourist.API.Repositories;

namespace Tourist.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthController> _logger;
        public AuthController(UserManager<ApplicationUser> userManager, IMapper mapper, ITokenRepository tokenRepository, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;       
            _tokenRepository = tokenRepository;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", request.Email);
            var user = _mapper.Map<ApplicationUser>(request);

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Reader");
                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return Ok(new { message = "User registered successfully" });
            }
            else
            {
                _logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}", request.Email, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                return BadRequest(identityResult.Errors);
            }

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", request.Email);

            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is not null)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(identityUser, request.Password);
                if (checkPasswordResult)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var jwtToken = await _tokenRepository.CreateJwtToken(identityUser, roles.ToList());
                    var refreshToken = await _tokenRepository.GenerateRefreshToken();
                    await _tokenRepository.SaveRefreshToken(identityUser, refreshToken);

                    var response = new LoginResponseDto
                    {
                        Email = request.Email,
                        Role = roles.FirstOrDefault(),
                        Token = jwtToken,
                        RefreshToken = refreshToken
                    };

                    _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", request.Email, response.Role);

                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Invalid password for email: {Email}", request.Email);
                }
            }
            else
            {
                _logger.LogWarning("Login attempt with unknown email: {Email}", request.Email);
            }
            return Unauthorized("Invalid Credentials");
        }
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("Refresh token endpoint called for email: {Email}", request.Email);

            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is null)
            {
                _logger.LogWarning("Refresh token attempt for invalid user: {Email}", request.Email);
                return Unauthorized("Invalid user");
            }

            var isValid = await _tokenRepository.ValidateRefreshToken(identityUser, request.RefreshToken);
            if (!isValid)
            {
                _logger.LogWarning("Invalid refresh token for email: {Email}", request.Email);
                return Unauthorized("Invalid refresh token");
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            var newJwtToken = await _tokenRepository.CreateJwtToken(identityUser, roles.ToList());
            var newRefreshToken = await _tokenRepository.GenerateRefreshToken();
            await _tokenRepository.SaveRefreshToken(identityUser, newRefreshToken);

            var response = new LoginResponseDto
            {
                Email = request.Email,
                Role = roles.FirstOrDefault(),
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };

            _logger.LogInformation("Refresh token issued for email: {Email}, Role: {Role}", request.Email, response.Role);

            return Ok(response);
        }
        [HttpGet("health")]
        public IActionResult Health()
        {
            _logger.LogInformation("Health check endpoint called.");
            return Ok("API is running");
        }
    }
}
