namespace EventMaster.Web.Models.Dashboard;

public class UserDashboardVm
{
    public MeDto Me { get; set; } = new();
    public List<DashboardBookingCardDto> Upcoming { get; set; } = new();
    public List<DashboardBookingCardDto> Past { get; set; } = new();
    public string? Error { get; set; }
}

public class MeDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
}

public class DashboardBookingCardDto
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }

    public string Name { get; set; } = "";
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }

    // This is what replaces price on the card:
    // Scheduled / Cancelled / Completed
    public string Status { get; set; } = "";

    public DateTime StartDateTimeUtc { get; set; }
    public string VenueName { get; set; } = "";
    public string VenueCity { get; set; } = "";

    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }

    public string BookingStatus { get; set; } = "";
}

public class BookingDetailsVm
{
    public BookingDetailsDto Booking { get; set; } = new();
    public PaymentSummaryDto? Payment { get; set; }
    public string? Error { get; set; }
}

public class BookingDetailsDto
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }

    public string EventName { get; set; } = "";
    public string Status { get; set; } = ""; // occurrence status

    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public string VenueName { get; set; } = "";
    public string VenueAddress { get; set; } = "";
    public string VenueCity { get; set; } = "";

    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
}

public class PaymentSummaryDto
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string Card { get; set; } = ""; // one-line detail is enough
}

public class UpdateProfileRequest
{
    public string CurrentPassword { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
