namespace EventMaster.Api.Entities;

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
