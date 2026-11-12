using HyperInsurance.Models;

namespace HyperInsurance.Services;

public static class InsuranceInputValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        InsuranceApplication application,
        Customer customer,
        Vehicle vehicle)
    {
        var errors = new Dictionary<string, string[]>();

        if (customer.YearsLicensed < 0)
        {
            errors[nameof(customer.YearsLicensed)] =
                ["Years licensed cannot be negative."];
        }

        if (customer.PreviousAccidents < 0)
        {
            errors[nameof(customer.PreviousAccidents)] =
                ["Previous accidents cannot be negative."];
        }

        if (customer.TrafficViolations < 0)
        {
            errors[nameof(customer.TrafficViolations)] =
                ["Traffic violations cannot be negative."];
        }

        if (vehicle.Value < 0)
        {
            errors[nameof(vehicle.Value)] = ["Vehicle value cannot be negative."];
        }

        if (vehicle.Horsepower < 0)
        {
            errors[nameof(vehicle.Horsepower)] =
                ["Horsepower cannot be negative."];
        }

        if (application.AnnualMileage < 0)
        {
            errors[nameof(application.AnnualMileage)] =
                ["Annual mileage cannot be negative."];
        }

        if (string.IsNullOrWhiteSpace(application.StorageType))
        {
            errors[nameof(application.StorageType)] =
                ["Storage type is required."];
        }

        if (string.IsNullOrWhiteSpace(application.Usage))
        {
            errors[nameof(application.Usage)] = ["Usage is required."];
        }

        return errors;
    }
}
