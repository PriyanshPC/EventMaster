namespace EventMaster.Api.Entities;
/// <summary>
///  Entity class representing the "venue" table in the database. This class is used by Entity Framework Core to map the "venue" table to a C# class, allowing for querying and manipulating venue data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "venue" table, such as venue_id, name, address, city, province, postal_code, capacity, seating, created_at, and updated_at. It also defines a navigation property for related entities, such as event_occurrences (the event occurrences that take place at this venue). This entity is an important part of the Events API as it allows organizers to manage venues for their events and provides customers with information about where events are taking place.
/// </summary>
public partial class venue
{
    public int venue_id { get; set; }

    public string name { get; set; } = null!;

    public string address { get; set; } = null!;

    public string city { get; set; } = null!;

    public string province { get; set; } = null!;

    public string postal_code { get; set; } = null!;

    public int capacity { get; set; }

    public bool seating { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<event_occurrence> event_occurrences { get; set; } = new List<event_occurrence>();
}
