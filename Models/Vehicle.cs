
namespace HyperInsurance.Models;

public class Vehicle
{
    public int Id { get; set; }

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal Value { get; set; }

    public int Horsepower { get; set; }

    public string Category { get; set; } = string.Empty;

    public ICollection<InsuranceApplication> Applications { get; set; }
        = new List<InsuranceApplication>();
}
