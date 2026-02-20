namespace EventMaster.Api.Entities;

public partial class review
{
    public int review_id { get; set; }

    public int occurrence_id { get; set; }

    public int customer_id { get; set; }

    public int rating { get; set; }

    public string? comment { get; set; }

    public DateTime created_at { get; set; }

    public virtual user customer { get; set; } = null!;

    public virtual event_occurrence occurrence { get; set; } = null!;

    public virtual ICollection<reply> replies { get; set; } = new List<reply>();
}
