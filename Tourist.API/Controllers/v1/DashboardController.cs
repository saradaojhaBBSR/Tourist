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
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, IMapper mapper, ILogger<DashboardController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Dashboard/getalltouristplaces
        [HttpGet("getalltouristplaces")]
        public async Task<ActionResult<IEnumerable<TouristPlacesDto>>> GetTouristPlaces()
        {
            _logger.LogInformation("Fetching all tourist places.");
            var places = await _context.TouristPlaces.ToListAsync();
            var placesDto = _mapper.Map<List<TouristPlacesDto>>(places);
            return Ok(placesDto);
        }
        
        // GET: api/Dashboard/gettouristplacebyid/{id}
        [HttpGet("gettouristplacebyid/{id}")]
        public async Task<ActionResult<TouristPlacesDto>> GetTouristPlace(int id)
        {
            _logger.LogInformation("Fetching tourist place with ID {TouristPlaceId}.", id);
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null)
            {
                _logger.LogWarning("Tourist place with ID {TouristPlaceId} not found.", id);
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
            _logger.LogInformation("Tourist place added by {UserEmail}: {@TouristPlace}", userEmail, entity);
            return CreatedAtAction(nameof(GetTouristPlace), new { id = entity.Id }, resultDto);
        }

        [HttpPut("updatetouristplace/{id}")]
        [Authorize(Roles = "Admin,Contributor")]
        public async Task<IActionResult> PutTouristPlace(int id, TouristPlacesDto touristPlace)
        {
            var entity = await _context.TouristPlaces.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Attempted to update non-existent tourist place with ID {TouristPlaceId}.", id);
                return NotFound();
            }

            _mapper.Map(touristPlace, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            // Set UpdatedBy to the logged-in user's email
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            entity.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Tourist place with ID {TouristPlaceId} updated by {UserEmail}.", id, userEmail);

            return NoContent();
        }

        [HttpDelete("deletetouristplace/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null)
            {
                _logger.LogWarning("Attempted to delete non-existent tourist place with ID {TouristPlaceId}.", id);
                return NotFound();
            }

            _context.TouristPlaces.Remove(place);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tourist place with ID {TouristPlaceId} deleted.", id);

            return NoContent();
        }
    }
}
