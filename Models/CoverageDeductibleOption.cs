namespace HyperInsurance.Models;

public class CoverageDeductibleOption
{
    public int Id { get; set; }

    public int CoverageId { get; set; }

    public Coverage? Coverage { get; set; }

    public decimal Deductible { get; set; }

    public decimal PremiumAdjustment { get; set; }
}
