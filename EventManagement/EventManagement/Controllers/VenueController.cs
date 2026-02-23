using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Api.Controllers
{
    /// <summary> /// Handles venue creation and listing all venues. /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VenueController : ControllerBase
    {
        private readonly VenueService _venueService;
        public VenueController(VenueService venueService) => _venueService = venueService;

        [HttpPost("create")]
        public async Task<IActionResult> CreateVenue(VenueCreateDto dto)
        {
            var venue = await _venueService.CreateVenueAsync(dto);
            return Ok(venue);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllVenues()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return Ok(venues);
        }
    }
}
