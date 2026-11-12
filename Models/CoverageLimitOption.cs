namespace HyperInsurance.Models;

public class CoverageLimitOption
{
    public int Id { get; set; }

    public int CoverageId { get; set; }

    public Coverage? Coverage { get; set; }

    public decimal CoverageLimit { get; set; }

    public decimal AdditionalPremium { get; set; }
}
