using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tourist.API.Data;
using Tourist.API.Models;
using Tourist.API.Models.Dto;

namespace Tourist.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]   
    [ResponseCache(Duration = 60)]
    [ApiVersion("1.0")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Dashboard/alltouristplaces
        [HttpGet("getalltouristplaces")]
        public async Task<ActionResult<IEnumerable<TouristPlacesDto>>> GetTouristPlaces()
        {
            var places = await _context.TouristPlaces.ToListAsync();
            var placesDto = _mapper.Map<List<TouristPlacesDto>>(places);
            return Ok(placesDto);
        }
        
        // GET: api/Dashboard/gettouristplacebyid/{id}
        [HttpGet("gettouristplacebyid/{id}")]
        public async Task<ActionResult<TouristPlacesDto>> GetTouristPlace(int id)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }
            var placeDto = _mapper.Map<TouristPlacesDto>(place);
            return Ok(placeDto);
        }

        [HttpPost("addtouristplace")]
        [Authorize(Roles = "Admin,Contributor")]
        public async Task<ActionResult<TouristPlacesDto>> PostTouristPlace(TouristPlacesDto touristPlace)
        {
            var entity = _mapper.Map<TouristPlaces>(touristPlace);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsActive = true;

            // Set CreatedBy and UpdatedBy to the logged-in user's email
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            entity.CreatedBy = userEmail;
            entity.UpdatedBy = userEmail;

            _context.TouristPlaces.Add(entity);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<TouristPlacesDto>(entity);
            return CreatedAtAction(nameof(GetTouristPlace), new { id = entity.Id }, resultDto);
        }

        [HttpPut("updatetouristplace/{id}")]
        [Authorize(Roles = "Admin,Contributor")]
        public async Task<IActionResult> PutTouristPlace(int id, TouristPlacesDto touristPlace)
        {
            var entity = await _context.TouristPlaces.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _mapper.Map(touristPlace, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            // Set UpdatedBy to the logged-in user's email
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            entity.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("deletetouristplace/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null)
            {
                return NotFound();
            }

            _context.TouristPlaces.Remove(place);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
