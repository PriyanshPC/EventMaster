namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "user" table in the database. This class is used by Entity Framework Core to map the "user" table to a C# class, allowing for querying and manipulating user data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "user" table, such as user_id, role, name, age, phone, email, username, password, status, created_at, and updated_at. It also defines navigation properties for related entities, such as _events (the events organized by this user), bookings (the bookings made by this user), replies (the replies made by this user), and reviews (the reviews left by this user). This entity is a fundamental part of the Events API and is used in various operations such as user registration, authentication, profile management, and associating users with their organized events, bookings, reviews, and replies.
/// </summary>
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
