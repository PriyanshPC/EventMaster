namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "event_occurrence" table in the database. This class is used by Entity Framework Core to map the "event_occurrence" table to a C# class, allowing for querying and manipulating event occurrence data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "event_occurrence" table, such as occurrence_id, event_id, date, time, venue_id, price, remaining_capacity, seats_occupied, status, created_at, and updated_at. It also defines navigation properties for related entities, such as _event (the event that this occurrence belongs to), bookings (the bookings made for this occurrence), reviews (the reviews left for this occurrence), and venue (the venue where this occurrence takes place). This entity is a key part of the Events API and is used in various operations such as creating event occurrences, retrieving occurrence details, updating occurrences, and managing bookings and reviews for occurrences.
/// </summary>
public partial class event_occurrence
{
    public int occurrence_id { get; set; }

    public int event_id { get; set; }

    public DateOnly date { get; set; }

    public TimeOnly time { get; set; }

    public int venue_id { get; set; }

    public decimal price { get; set; }

    public int remaining_capacity { get; set; }

    public string? seats_occupied { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual _event _event { get; set; } = null!;

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    public virtual venue venue { get; set; } = null!;
}
