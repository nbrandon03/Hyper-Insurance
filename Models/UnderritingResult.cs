namespace HyperInsurance.Models;

public class UnderwritingResult
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public InsuranceApplication? Application { get; set; }

    public string Decision { get; set; } = UnderwritingDecisions.Pending;

    public decimal RiskScore { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UnderwritingRiskFactor> RiskFactors { get; set; }
        = new List<UnderwritingRiskFactor>();

    // Manual underwriting review information
    public string? ReviewedBy { get; set; }

    public string? ReviewNotes { get; set; }

    public DateTime? ReviewedAt { get; set; }
}