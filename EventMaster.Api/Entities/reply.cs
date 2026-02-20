namespace EventMaster.Api.Entities;

public partial class reply
{
    public int reply_id { get; set; }

    public int review_id { get; set; }

    public int organizer_id { get; set; }

    public string reply_text { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual user organizer { get; set; } = null!;

    public virtual review review { get; set; } = null!;
}
