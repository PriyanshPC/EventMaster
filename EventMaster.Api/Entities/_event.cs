namespace EventMaster.Api.Entities;

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
