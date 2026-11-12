
namespace HyperInsurance.Models;

public class Customer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int YearsLicensed { get; set; }

    public int PreviousAccidents { get; set; }

    public int TrafficViolations { get; set; }

    public ICollection<InsuranceApplication> Applications { get; set; }
        = new List<InsuranceApplication>();
}
