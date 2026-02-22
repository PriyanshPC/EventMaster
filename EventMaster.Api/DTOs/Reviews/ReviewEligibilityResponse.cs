namespace EventMaster.Api.DTOs.Reviews;

public class ReviewEligibilityResponse
{
    public bool CanAddReview { get; set; }
    public int? EligibleOccurrenceId { get; set; }
}
