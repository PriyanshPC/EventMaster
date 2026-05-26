namespace EventMaster.Web.Models
{
    /// <summary>
    /// ViewModel for the error page, which displays error information when an unhandled exception occurs in the application. Contains a RequestId property to track the specific request that caused the error, and a ShowRequestId property to determine whether to display the RequestId in the UI. This ViewModel is used by the Error view to show error details to the user and can be populated with relevant information when an error occurs in the application.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
