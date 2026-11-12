namespace HyperInsurance.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>Coverage selections to include when issuing a policy.</summary>
public sealed record IssuePolicyRequest
{
    [Required, MaxLength(50)]
    public required List<PolicyCoverageSelectionRequest> Coverages { get; init; }
}

/// <summary>Specifies the selected limit and deductible for one coverage.</summary>
public sealed record PolicyCoverageSelectionRequest
{
    [Range(1, int.MaxValue)]
    public required int CoverageId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public required decimal CoverageLimit { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public required decimal Deductible { get; init; }
}