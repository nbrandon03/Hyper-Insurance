
namespace HyperInsurance.Models;

public class InsuranceApplication
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public string Usage { get; set; } = string.Empty;

    public int AnnualMileage { get; set; }

    public string StorageType { get; set; } = string.Empty;

    public string Status { get; set; } = ApplicationStatuses.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Policy> Policies { get; set; }
        = new List<Policy>();

    public ICollection<UnderwritingResult> UnderwritingResults { get; set; }
        = new List<UnderwritingResult>();
}
