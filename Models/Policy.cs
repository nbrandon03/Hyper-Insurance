
namespace HyperInsurance.Models;

public class Policy
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public InsuranceApplication? Application { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public decimal AnnualPremium { get; set; }

    public decimal? Deductible { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public string Status { get; set; } = PolicyStatuses.Active;

    public ICollection<PolicyCoverage> Coverages { get; set; }
    = new List<PolicyCoverage>();
}
