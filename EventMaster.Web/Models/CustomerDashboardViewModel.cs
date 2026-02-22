namespace EventMaster.Web.Models;

public class CustomerDashboardViewModel
{
    public string ActiveTab { get; set; } = "bookings";
    public MeSettingsViewModel Settings { get; set; } = new();
    public List<DashboardBookingCardViewModel> UpcomingBookings { get; set; } = new();
    public List<DashboardBookingCardViewModel> PastBookings { get; set; } = new();
    public string? Message { get; set; }
    public string? Error { get; set; }
}

public class MeSettingsViewModel
{
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string EditMode { get; set; } = "";
}

public class DashboardBookingCardViewModel
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubTitle { get; set; } = "";
    public string VenueLine { get; set; } = "";
    public string Status { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}
