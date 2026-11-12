using HyperInsurance.Models;

namespace HyperInsurance.Services;

public sealed record UnderwritingEvaluationContext(
    InsuranceApplication Application,
    Customer Customer,
    Vehicle Vehicle);

public interface IUnderwritingRule
{
    IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context);
}

public sealed class DriverHistoryRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        var factors = new List<UnderwritingRiskFactor>();
        if (context.Customer.PreviousAccidents > 0)
        {
            factors.Add(new UnderwritingRiskFactor
            {
                Rule = "Driver history",
                Explanation = $"{context.Customer.PreviousAccidents} previous accident(s).",
                Points = context.Customer.PreviousAccidents * 15m
            });
        }

        if (context.Customer.TrafficViolations > 0)
        {
            factors.Add(new UnderwritingRiskFactor
            {
                Rule = "Driver history",
                Explanation = $"{context.Customer.TrafficViolations} traffic violation(s).",
                Points = context.Customer.TrafficViolations * 5m
            });
        }

        return factors;
    }
}

public sealed class DrivingExperienceRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        var points = context.Customer.YearsLicensed switch
        {
            < 2 => 20m,
            < 5 => 10m,
            _ => 0m
        };

        return points == 0
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Driving experience",
                    Explanation = $"Driver has {context.Customer.YearsLicensed} year(s) of licensed experience.",
                    Points = points
                }
            };
    }
}

public sealed class VehiclePerformanceRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        var points = context.Vehicle.Horsepower switch
        {
            >= 700 => 25m,
            >= 500 => 15m,
            _ => 0m
        };

        return points == 0
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Vehicle performance",
                    Explanation = $"Vehicle has {context.Vehicle.Horsepower} horsepower.",
                    Points = points
                }
            };
    }
}

public sealed class VehicleValueRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        var points = context.Vehicle.Value switch
        {
            >= 1000000m => 30m,
            >= 250000m => 20m,
            _ => 0m
        };

        return points == 0
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Vehicle value",
                    Explanation = $"Vehicle value is {context.Vehicle.Value:C}.",
                    Points = points
                }
            };
    }
}

public sealed class AnnualMileageRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        return context.Application.AnnualMileage <= 30000
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Annual mileage",
                    Explanation = $"Annual mileage is {context.Application.AnnualMileage:N0}, above 30,000.",
                    Points = 15m
                }
            };
    }
}

public sealed class OutdoorStorageRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        return !string.Equals(
                context.Application.StorageType,
                VehicleStorageTypes.Outdoor,
                StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Vehicle storage",
                    Explanation = "Vehicle is stored outdoors.",
                    Points = 10m
                }
            };
    }
}

public sealed class TrackUsageRule : IUnderwritingRule
{
    public IReadOnlyList<UnderwritingRiskFactor> Evaluate(
        UnderwritingEvaluationContext context)
    {
        return !string.Equals(
                context.Application.Usage,
                VehicleUsages.Track,
                StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<UnderwritingRiskFactor>()
            : new[]
            {
                new UnderwritingRiskFactor
                {
                    Rule = "Vehicle usage",
                    Explanation = "Track usage requires specialist underwriting review.",
                    Points = 0m
                }
            };
    }
}
