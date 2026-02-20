using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Venues;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly EventMasterDbContext _db;

    public VenuesController(EventMasterDbContext db, PasswordHasher hasher, JwtTokenService jwt, CurrentUser me)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenues()
    {
        var venues = await _db.venues
            .Select(v => new VenueResponse
            {
                VenueId = v.venue_id,
                Name = v.name,
                Address = v.address,
                City = v.city,
                Province = v.province,
                PostalCode = v.postal_code,
                Capacity = v.capacity,
                Seating = v.seating
            })
            .ToListAsync();

        return Ok(venues);
    }

    // GET: api/venues/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenue(int id)
    {
        var venue = await _db.venues
            .Where(v => v.venue_id == id)
            .Select(v => new VenueResponse
            {
                VenueId = v.venue_id,
                Name = v.name,
                Address = v.address,
                City = v.city,
                Province = v.province,
                PostalCode = v.postal_code,
                Capacity = v.capacity,
                Seating = v.seating
            })
            .FirstOrDefaultAsync();

        if (venue == null)
            return NotFound("Venue not found");

        return Ok(venue);
    }
}
