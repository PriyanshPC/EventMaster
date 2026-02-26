namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "event" table in the database. This class is used by Entity Framework Core to map the "event" table to a C# class, allowing for querying and manipulating event data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "event" table, such as event_id, org_id, name, category, description, image, created_at, and updated_at. It also defines navigation properties for related entities, such as event_occurrences (the occurrences of this event) and org (the organizer of this event). This entity is a central part of the Events API and is used in various operations such as creating events, retrieving event details, updating events, and deleting events.
/// </summary>
public partial class _event
{
    public int event_id { get; set; }

    public int org_id { get; set; }

    public string name { get; set; } = null!;

    public string category { get; set; } = null!;

    public string? description { get; set; }

    public string? image { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<event_occurrence> event_occurrences { get; set; } = new List<event_occurrence>();

    public virtual user org { get; set; } = null!;
}
