namespace EventMaster.Api.Entities;

public partial class booking
{
    public int booking_id { get; set; }

    public int occurrence_id { get; set; }

    public int customer_id { get; set; }

    public int quantity { get; set; }

    public string? seats_occupied { get; set; }

    public string status { get; set; } = null!;

    public decimal total_amount { get; set; }

    public string ticket_number { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user customer { get; set; } = null!;

    public virtual event_occurrence occurrence { get; set; } = null!;

    public virtual ICollection<payment> payments { get; set; } = new List<payment>();
}
