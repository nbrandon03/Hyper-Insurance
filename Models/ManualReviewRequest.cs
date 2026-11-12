using System.ComponentModel.DataAnnotations;

namespace HyperInsurance.Models;

/// <summary>Records the decision and notes from a manual underwriting review.</summary>
public sealed record ManualReviewRequest
{
    [Required]
    [RegularExpression(UnderwritingDecisions.ManualReviewDecisionPattern)]
    public required string Decision { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string ReviewedBy { get; init; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string ReviewNotes { get; init; }
}