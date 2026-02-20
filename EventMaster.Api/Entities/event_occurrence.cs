namespace EventMaster.Api.Entities;

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
