namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "review" table in the database. This class is used by Entity Framework Core to map the "review" table to a C# class, allowing for querying and manipulating review data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "review" table, such as review_id, occurrence_id, customer_id, rating, comment, created_at, and navigation properties for the related customer (user), occurrence (event occurrence), and replies (the replies to this review). This entity is an important part of the Events API as it allows customers to leave reviews for event occurrences they have attended, providing valuable feedback for organizers and other customers. Organizers can also respond to reviews through the reply entity, fostering engagement between organizers and customers.
/// </summary>
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
