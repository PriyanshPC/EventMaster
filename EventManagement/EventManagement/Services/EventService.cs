using EventTicketManagement.Api.Data;
using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventTicketManagement.Api.Services
{
    /// <summary> 
    ///  Handles creating events, adding occurrences, and fetching event lists. 
    ///  </summary>
    public class EventService
    {
        private readonly AppDbContext _context;
        public EventService(AppDbContext context)
        {
            _context = context;
        }
        /// Creates a new event for an organizer.
        public async Task<Event> CreateEventAsync(EventCreateDto dto, int organizerId)
        {
            var ev = new Event
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                OrgId = organizerId,
                Image = "default.png"
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return ev;
        }
        /// Creates a new event occurrence (date, time, venue, capacity).
        public async Task<EventOccurrence> CreateOccurrenceAsync(int eventId, OccurrenceCreateDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                throw new Exception("Event not found");

            // Verify venue exists
            var venue = await _context.Venues.FindAsync(dto.VenueId);
            if (venue == null)
                throw new Exception("Venue not found");
            var occurrence = new EventOccurrence
            {
                EventId = eventId,
                Date = dto.Date,
                Time = dto.Time,
                Price = dto.Price,
                RemainingCapacity = dto.RemainingCapacity,
                Status = dto.Status,
                VenueId = dto.VenueId
            };
            _context.EventOccurrences.Add(occurrence);
            await _context.SaveChangesAsync();
            return occurrence;
        }
        /// Gets all events created by a specific organizer.
        public async Task<List<Event>> GetEventsByOrganizerAsync(int organizerId)
        {
            return await _context.Events
                .Where(e => e.OrgId == organizerId)
                .Include(e => e.Occurrences)
                .ToListAsync();
        }
        /// Gets all upcoming event occurrences
        public async Task<List<UpcomingEventDto>> GetUpcomingOccurrencesAsync()
        {
            return await _context.EventOccurrences
                .Include(o => o.Event)
                .Include(o => o.Venue)
                .Where(o => o.Status == "Scheduled" &&
                            o.Date >= DateOnly.FromDateTime(DateTime.UtcNow))
                .OrderBy(o => o.Date)
                .ThenBy(o => o.Time)
                .Select(o => new UpcomingEventDto
                {
                    OccurrenceId = o.OccurrenceId,
                    EventName = o.Event.Name,
                    Category = o.Event.Category,
                    VenueName = o.Venue.Name,
                    Date = o.Date,
                    Time = o.Time,
                    Price = o.Price,
                    RemainingCapacity = o.RemainingCapacity
                })
                .ToListAsync();
        }


    }
}
