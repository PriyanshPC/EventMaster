using EventTicketManagement.Api.Data;
using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventTicketManagement.Api.Services
{
    /// <summary>
    /// Service for managing venues. Provides methods to create a new venue and retrieve all venues from the database. Uses Entity Framework Core for data access and manipulation.
    /// </summary>
    public class VenueService
    {
        private readonly AppDbContext _context;
        public VenueService(AppDbContext context)
        {
            _context = context;
        }
/// <summary>
/// Creates a new venue based on the provided VenueCreateDto. Maps the DTO properties to a new Venue entity, adds it to the database context, and saves the changes. Returns the created Venue object with its assigned ID.
/// </summary>
/// <param name="dto"></param>
/// <returns></returns>
        public async Task<Venue> CreateVenueAsync(VenueCreateDto dto)
        {
            var venue = new Venue
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                Province = dto.Province,
                PostalCode = dto.PostalCode,
                Capacity = dto.Capacity,
                Seating = dto.Seating
            };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return venue;
        }
/// <summary>
/// Retrieves all venues from the database. Executes an asynchronous query to fetch all Venue entities and returns them as a list. This method allows clients to get a complete list of available venues for event management purposes.
/// </summary>
/// <returns></returns>
        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.ToListAsync();
        }
    }
}
