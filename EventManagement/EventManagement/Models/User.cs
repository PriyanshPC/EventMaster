namespace EventTicketManagement.Api.Models
{public class User
{
    public int UserId { get; set; }

    public string Role { get; set; } = null!; // CUSTOMER / ORGANIZER
    public string Name { get; set; } = null!;
    public int Age { get; set; }

    public string? Phone { get; set; }

    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Event>? OrganizedEvents { get; set; }
}
}