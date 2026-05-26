namespace EventMaster.Api.DTOs.Venues;
/// <summary>
/// Represents the response object for a venue, containing details about the venue such as its name, address, capacity, and seating arrangement.
/// </summary>
public class VenueResponse
{
    public int VenueId { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Province { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public int Capacity { get; set; }
    public bool Seating { get; set; }
}