namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "reply" table in the database. This class is used by Entity Framework Core to map the "reply" table to a C# class, allowing for querying and manipulating reply data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "reply" table, such as reply_id, review_id, organizer_id, reply_text, created_at, and navigation properties for the related organizer (user) and review entities. This entity is an important part of the Events API as it allows organizers to respond to customer reviews, providing a way for organizers to engage with their customers and address any feedback or concerns raised in reviews.
/// </summary>
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
