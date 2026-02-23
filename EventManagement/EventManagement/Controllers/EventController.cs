using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        public EventController(EventService eventService) => _eventService = eventService;

        // POST: api/event/create
        // Body JSON example:
        // {
        //   "name": "Concert",
        //   "category": "Music",
        //   "description": "Live concert",
        //   "organizerId": 1
        // }
        [HttpPost("create")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            int organizerId = dto.OrganizerId; // get organizerId from DTO
            var ev = await _eventService.CreateEventAsync(dto, organizerId);
            return Ok(ev);
        }

        // POST: api/event/{eventId}/occurrence
        // Body JSON example:
        // {
        //   "eventId": 1,
        //   "date": "2026-03-01",
        //   "time": "19:00",
        //   "price": 50,
        //   "remainingCapacity": 100,
        //   "status": "Active",
        //   "venueId": 1
        // }
        [HttpPost("{eventId}/occurrence")]
public async Task<IActionResult> CreateOccurrence(int eventId, [FromBody] OccurrenceCreateDto dto)
{
    var occurrence = await _eventService.CreateOccurrenceAsync(eventId, dto);

    return Ok(new
    {
        occurrence.OccurrenceId,
        occurrence.EventId,
        occurrence.VenueId,
        occurrence.Date,
        occurrence.Time,
        occurrence.Price,
        occurrence.RemainingCapacity,
        occurrence.Status
    });
}

        // GET: api/event/organizer/1
        [HttpGet("organizer/{organizerId}")]
        public async Task<IActionResult> GetEventsByOrganizer(int organizerId)
        {
            var events = await _eventService.GetEventsByOrganizerAsync(organizerId);
            return Ok(events);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingOccurrences()
        {
            var occurrences = await _eventService.GetUpcomingOccurrencesAsync();
            return Ok(occurrences);
        }

        }
}
