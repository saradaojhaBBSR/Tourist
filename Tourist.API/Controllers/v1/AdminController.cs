using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tourist.API.Models;
using Tourist.API.Models.Dto;

namespace Tourist.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger;
        public AdminController(UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = _userManager.Users.ToList();    
            var result = new List<UsersInfoDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<UsersInfoDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Role = roles.FirstOrDefault();      
                result.Add(dto);
            }

            _logger.LogInformation("Fetched {Count} users", result.Count);
            return Ok(result);
        }   

        [HttpPatch("users/{id}")]
        public async Task<IActionResult> PatchUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            if (updateDto == null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Patch failed: User with id {UserId} not found", id);
                return NotFound();
            }

            // Update user fields if provided
            if (updateDto.FirstName != null) user.FirstName = updateDto.FirstName;
            if (updateDto.MiddleName != null) user.MiddleName = updateDto.MiddleName;
            if (updateDto.LastName != null) user.LastName = updateDto.LastName;
            if (updateDto.Email != null)
            {
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
            }
            if (updateDto.PhoneNumber != null) user.PhoneNumber = updateDto.PhoneNumber;
            if (updateDto.Country != null) user.Country = updateDto.Country;
            if (updateDto.State != null) user.State = updateDto.State;
            if (updateDto.City != null) user.City = updateDto.City;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Patch failed: Could not update user {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            // Handle role update if provided
            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, updateDto.Role);
                _logger.LogInformation("User {UserId} role updated to {Role}", id, updateDto.Role);
            }
            else
            {
                _logger.LogInformation("User {UserId} updated", id);
            }

            var dto = _mapper.Map<UsersInfoDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Role = roles.FirstOrDefault();
            return Ok(dto);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Delete failed: User with id {UserId} not found", id);
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Delete failed: Could not delete user {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }
            _logger.LogInformation("User {UserId} deleted", id);
            return NoContent();
        }
    }
}
