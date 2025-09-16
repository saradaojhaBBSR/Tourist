using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
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
        public AdminController(UserManager<ApplicationUser> userManager,IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();    
            var result = new List<UsersInfoDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<UsersInfoDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Role = roles.FirstOrDefault();      
                result.Add(dto);
            }

            return Ok(result);
        }   

        [HttpPatch("users/{id}")]
        public async Task<IActionResult> PatchUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            if (updateDto == null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Handle role update if provided
            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, updateDto.Role);
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
            if (user == null) return NotFound();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return NoContent();
        }
    }
}
