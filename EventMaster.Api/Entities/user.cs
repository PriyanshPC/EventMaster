namespace EventMaster.Api.Entities;

public partial class user
{
    public int user_id { get; set; }

    public string role { get; set; } = null!;

    public string name { get; set; } = null!;

    public int age { get; set; }

    public string? phone { get; set; }

    public string email { get; set; } = null!;

    public string username { get; set; } = null!;

    public string password { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<_event> _events { get; set; } = new List<_event>();

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual ICollection<reply> replies { get; set; } = new List<reply>();

    public virtual ICollection<review> reviews { get; set; } = new List<review>();
}
