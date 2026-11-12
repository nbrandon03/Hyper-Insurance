namespace HyperInsurance.Models;

public class PolicyCoverage
{
    public int Id { get; set; }

    public int PolicyId { get; set; }
    public Policy? Policy { get; set; }

    public int CoverageId { get; set; }
    public Coverage? Coverage { get; set; }

    public string CoverageName { get; set; } = string.Empty;

    public string CoverageDescription { get; set; } = string.Empty;

    // Preserve the coverage terms when the policy is issued.
    public decimal CoverageLimit { get; set; }

    public decimal Deductible { get; set; }

    // Preserve the pricing terms when the policy is issued.
    public decimal LimitPremium { get; set; }

    public decimal DeductiblePremiumAdjustment { get; set; }

    public decimal AdditionalPremium { get; set; }
}