using System.ComponentModel.DataAnnotations;

namespace EventTicketManagement.Api.Models
{
    public class OrganizerProfile
    {
        [Key]
        public int OrganizerId { get; set; }
        public int UserId { get; set; }
        public required string CompanyName { get; set; }
    }
}
