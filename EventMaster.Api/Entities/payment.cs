namespace EventMaster.Api.Entities;

public partial class payment
{
    public int payment_id { get; set; }

    public int booking_id { get; set; }

    public decimal amount { get; set; }

    public string? card { get; set; }

    public string status { get; set; } = null!;

    public string? details { get; set; }

    public DateTime created_at { get; set; }

    public virtual booking booking { get; set; } = null!;
}
