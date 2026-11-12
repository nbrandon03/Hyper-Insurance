using HyperInsurance.Models;

namespace HyperInsurance.Services;

public sealed class RatingService
{
    public RatingCalculation Calculate(
        Customer customer,
        Vehicle vehicle,
        InsuranceApplication application,
        IEnumerable<PolicyCoverage>? selectedCoverages = null)
    {
        var components = new List<PremiumComponent>
        {
            new("Base premium", 1500m),
            new("Vehicle value", vehicle.Value * 0.015m),
            new("Vehicle horsepower", vehicle.Horsepower * 2m),
            new("Previous accidents", customer.PreviousAccidents * 500m),
            new("Traffic violations", customer.TrafficViolations * 150m),
            new("Driving experience", customer.YearsLicensed switch
            {
                < 2 => 1000m,
                < 5 => 500m,
                _ => 0m
            }),
            new("Annual mileage", application.AnnualMileage > 30000 ? 750m : 0m),
            new("Vehicle storage", string.Equals(
                application.StorageType,
                VehicleStorageTypes.Outdoor,
                StringComparison.OrdinalIgnoreCase) ? 300m : 0m),
            new("Vehicle usage", string.Equals(
                application.Usage,
                VehicleUsages.Track,
                StringComparison.OrdinalIgnoreCase) ? 2000m : 0m)
        };

        if (selectedCoverages is not null)
        {
            components.AddRange(selectedCoverages.Select(coverage =>
                new PremiumComponent(
                    $"Coverage {coverage.CoverageName} " +
                    $"(limit {coverage.CoverageLimit:0.##}, deductible {coverage.Deductible:0.##})",
                    coverage.AdditionalPremium)));
        }

        var roundedBreakdown = components
            .Select(component => component with
            {
                Amount = Math.Round(component.Amount, 2)
            })
            .ToList();
        var annualPremium = Math.Round(
            roundedBreakdown.Sum(component => component.Amount),
            2);

        return new RatingCalculation(annualPremium, roundedBreakdown);
    }

    public decimal CalculateAnnualPremium(
        Customer customer,
        Vehicle vehicle,
        InsuranceApplication application,
        IEnumerable<PolicyCoverage>? selectedCoverages = null)
    {
        return Calculate(customer, vehicle, application, selectedCoverages)
            .AnnualPremium;
    }
}
