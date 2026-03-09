namespace EventMaster.Api.DTOs.Reviews;
/// <summary>
/// DTO for representing the eligibility of a user to add a review for an event.
/// </summary>
public class ReviewEligibilityResponse
{
    public bool CanAddReview { get; set; }
    public int? EligibleOccurrenceId { get; set; }
}
