namespace EventMaster.Api.Entities;
/// <summary>
/// Entity class representing the "payment" table in the database. This class is used by Entity Framework Core to map the "payment" table to a C# class, allowing for querying and manipulating payment data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "payment" table, such as payment_id, booking_id, amount, card, status, details, created_at, and a navigation property for the related booking entity. This entity is an important part of the Events API and is used in various operations such as creating payments, retrieving payment details, updating payment status, and managing payment information for bookings.
/// </summary>
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
