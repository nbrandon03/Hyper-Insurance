namespace HyperInsurance.Models;

public class UnderwritingRiskFactor
{
    public int Id { get; set; }

    public int UnderwritingResultId { get; set; }

    public UnderwritingResult? UnderwritingResult { get; set; }

    public string Rule { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public decimal Points { get; set; }
}
