using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hyper_Insurance.Tests;

[TestClass]
public sealed class UnderwritingAndRatingTests : DatabaseTestBase
{
    [TestMethod]
    public async Task StandardDriverAndVehicleAreApprovedWithZeroRiskScore()
    {
        var records = await CreateApplicationAsync();
        var result = CreateUnderwritingService().EvaluateApplication(
            records.Application,
            records.Customer,
            records.Vehicle);

        Assert.AreEqual(UnderwritingDecisions.Approved, result.Decision);
        Assert.AreEqual(0m, result.RiskScore);
        Assert.IsTrue(result.Reason.Contains("standard underwriting criteria", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task HighPerformanceVehicleAndInexperiencedDriverAreReferredWithExplanations()
    {
        var records = await CreateApplicationAsync(
            yearsLicensed: 1,
            vehicleValue: 300000m,
            horsepower: 650);

        var result = CreateUnderwritingService().EvaluateApplication(
            records.Application,
            records.Customer,
            records.Vehicle);

        Assert.AreEqual(UnderwritingDecisions.Referral, result.Decision);
        Assert.AreEqual(55m, result.RiskScore);
        CollectionAssert.AreEquivalent(
            new[] { "Driving experience", "Vehicle performance", "Vehicle value" },
            result.RiskFactors.Select(factor => factor.Rule).ToArray());
        Assert.IsTrue(result.Reason.Contains("referral threshold", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task SevereDriverHistoryAndPerformanceAreDeclined()
    {
        var records = await CreateApplicationAsync(
            accidents: 4,
            horsepower: 700);

        var result = CreateUnderwritingService().EvaluateApplication(
            records.Application,
            records.Customer,
            records.Vehicle);

        Assert.AreEqual(UnderwritingDecisions.Declined, result.Decision);
        Assert.AreEqual(85m, result.RiskScore);
        Assert.IsTrue(result.Reason.Contains("decline threshold", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task TrackUsageAlwaysRequiresSpecialistReferral()
    {
        var records = await CreateApplicationAsync(usage: VehicleUsages.Track);

        var result = CreateUnderwritingService().EvaluateApplication(
            records.Application,
            records.Customer,
            records.Vehicle);

        Assert.AreEqual(UnderwritingDecisions.Referral, result.Decision);
        Assert.IsTrue(result.Reason.Contains("Specialist review is required", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task RatingIncludesVehicleDriverUsageAndSelectedCoverageComponents()
    {
        var records = await CreateApplicationAsync(
            yearsLicensed: 1,
            accidents: 1,
            violations: 2,
            annualMileage: 35000,
            storageType: VehicleStorageTypes.Outdoor,
            usage: VehicleUsages.Track);
        var coverage = new PolicyCoverage
        {
            CoverageName = "Collision",
            CoverageLimit = 50000m,
            Deductible = 1000m,
            AdditionalPremium = 175m
        };

        var calculation = new RatingService().Calculate(
            records.Customer,
            records.Vehicle,
            records.Application,
            [coverage]);

        Assert.AreEqual(7275m, calculation.AnnualPremium);
        Assert.AreEqual(10, calculation.Breakdown.Count);
        Assert.AreEqual(175m, calculation.Breakdown[^1].Amount);
    }

    private static UnderwritingService CreateUnderwritingService()
    {
        return new UnderwritingService(
        [
            new DriverHistoryRule(),
            new DrivingExperienceRule(),
            new VehiclePerformanceRule(),
            new VehicleValueRule(),
            new AnnualMileageRule(),
            new OutdoorStorageRule(),
            new TrackUsageRule()
        ]);
    }
}
