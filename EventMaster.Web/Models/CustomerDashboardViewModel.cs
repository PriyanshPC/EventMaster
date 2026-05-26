namespace EventMaster.Web.Models;
/// <summary>
/// ViewModel for the customer dashboard page, which includes the user's profile settings and lists of their upcoming and past bookings. Contains properties for the active tab (bookings or settings), user profile information, lists of upcoming and past bookings with relevant details for each booking, and optional message and error properties for displaying feedback to the user. This ViewModel is populated by calling the Auth API to fetch the user's profile information and the Bookings API to fetch their bookings, which are then categorized into upcoming and past based on the current date. The ViewModel is used to render the customer dashboard page with all the necessary information and controls for managing their bookings and profile settings.
/// </summary>
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
