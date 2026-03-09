namespace EventMaster.Api.Entities;
/// <summary>
///  Entity class representing the "booking" table in the database. This class is used by Entity Framework Core to map the "booking" table to a C# class, allowing for querying and manipulating booking data in the database using LINQ and EF Core's DbContext. The class includes properties that correspond to the columns in the "booking" table, such as booking_id, occurrence_id, customer_id, quantity, seats_occupied, status, total_amount, ticket_number, created_at, and updated_at. It also defines navigation properties for related entities, such as customer (the user who made the booking), occurrence (the event occurrence that was booked), and payments (the payments associated with this booking). This entity is an important part of the Events API and is used in various operations such as creating bookings, retrieving booking details, updating bookings, and managing payments for bookings.
/// </summary>
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
