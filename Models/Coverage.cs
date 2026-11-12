namespace HyperInsurance.Models;

public class Coverage
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<CoverageLimitOption> LimitOptions { get; set; }
        = new List<CoverageLimitOption>();

    public ICollection<CoverageDeductibleOption> DeductibleOptions { get; set; }
        = new List<CoverageDeductibleOption>();
}