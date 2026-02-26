using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Venues;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/venues")]
/// <summary>
/// Controller for managing venues. This includes endpoints to retrieve a list of all venues and to get
/// details of a specific venue by its ID. Both endpoints are public and do not require authentication, allowing anyone to view venue information. The controller uses the EventMasterDbContext to access the venues data from the database and returns the relevant information in the response. This allows users to browse available venues when planning events.
/// </summary>
public class VenuesController : ControllerBase
{
    private readonly EventMasterDbContext _db;

    public VenuesController(EventMasterDbContext db, PasswordHasher hasher, JwtTokenService jwt, CurrentUser me)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/venues
    /// Retrieves a list of all venues with their basic details. This endpoint is public and does
    /// not require authentication, allowing anyone to view the available venues. 
    /// The response includes an array of venues, each containing its ID, name, address, city, province, postal code, capacity, and seating arrangement. This allows users to browse through the venues when planning their events and helps them make informed decisions about where to host their events.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// GET /api/venues/{id}
    /// Retrieves the details of a specific venue by its ID. This endpoint is public and does
    /// not require authentication, allowing anyone to view the information of a venue. The response includes all relevant details about the venue, such as its name, address, city, province, postal code, capacity, and seating arrangement. If the venue with the specified ID does not exist, it returns a 404 error. This allows users to get detailed information about a specific venue when planning their events.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
