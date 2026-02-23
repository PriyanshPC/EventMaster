namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for upcoming events. Contains properties that represent the details of an upcoming event occurrence, including the occurrence ID, event name, category, venue name, date, time, price, and remaining capacity. This DTO is used to transfer upcoming event data from the server to the client in a structured format, allowing for easy consumption and display of upcoming event information in the client application.
    /// </summary>
    public class UpcomingEventDto
    {
        public int OccurrenceId { get; set; }
        public string EventName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string VenueName { get; set; } = null!;
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public decimal Price { get; set; }
        public int RemainingCapacity { get; set; }
    }
}
