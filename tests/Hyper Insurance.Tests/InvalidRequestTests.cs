using HyperInsurance.Controllers;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;

namespace Hyper_Insurance.Tests;

[TestClass]
public sealed class InvalidRequestTests : DatabaseTestBase
{
    [TestMethod]
    public async Task ApplicationWithMissingCustomerReturnsNotFound()
    {
        var vehicle = new Vehicle
        {
            Make = "Test",
            Model = "Vehicle",
            Year = 2024,
            Value = 20000m,
            Horsepower = 150,
            Category = "Standard"
        };
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var controller = new ApplicationsController(Context);
        ConfigureController(controller);
        var result = await controller.CreateApplication(
            new CreateInsuranceApplicationRequest
            {
                CustomerId = 999,
                VehicleId = vehicle.Id,
                Usage = VehicleUsages.Personal,
                AnnualMileage = 10000,
                StorageType = VehicleStorageTypes.Garage
            },
            CancellationToken.None);

        Assert.AreEqual(404, GetStatusCode(result));
    }

    [TestMethod]
    public void InvalidApplicationValuesFailRequestValidation()
    {
        var request = new CreateInsuranceApplicationRequest
        {
            CustomerId = 0,
            VehicleId = 0,
            Usage = string.Empty,
            AnnualMileage = -1,
            StorageType = string.Empty
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.IsFalse(isValid);
        Assert.IsTrue(validationResults.Count >= 5);
    }

    [TestMethod]
    public async Task ApplicationWithMissingVehicleReturnsNotFound()
    {
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = $"customer{Guid.NewGuid():N}@example.test",
            YearsLicensed = 10
        };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        var controller = new ApplicationsController(Context);
        ConfigureController(controller);
        var result = await controller.CreateApplication(
            new CreateInsuranceApplicationRequest
            {
                CustomerId = customer.Id,
                VehicleId = 999,
                Usage = VehicleUsages.Personal,
                AnnualMileage = 10000,
                StorageType = VehicleStorageTypes.Garage
            },
            CancellationToken.None);

        Assert.AreEqual(404, GetStatusCode(result));
    }

    [TestMethod]
    public async Task MissingApplicationAndPolicyRecordsReturnNotFound()
    {
        var ratingController = new RatingController(Context, new RatingService());
        ConfigureController(ratingController);
        Assert.AreEqual(
            404,
            GetStatusCode(await ratingController.CalculateQuote(999, CancellationToken.None)));

        var policiesController = new PoliciesController(Context, new RatingService());
        ConfigureController(policiesController);
        Assert.AreEqual(
            404,
            GetStatusCode(await policiesController.GetPolicyById(999, CancellationToken.None)));
        Assert.AreEqual(
            404,
            GetStatusCode(await policiesController.CancelPolicy(999, CancellationToken.None)));
        Assert.AreEqual(
            404,
            GetStatusCode(await policiesController.RenewPolicy(999, CancellationToken.None)));
    }

    [TestMethod]
    public async Task PolicyCannotBeIssuedForUnapprovedApplication()
    {
        var records = await CreateApplicationAsync(status: ApplicationStatuses.Pending);
        var coverage = await CreateCoverageAsync();
        var controller = new PoliciesController(Context, new RatingService());
        ConfigureController(controller);

        var result = await controller.IssuePolicy(
            records.Application.Id,
            new IssuePolicyRequest
            {
                Coverages =
                [
                    new PolicyCoverageSelectionRequest
                    {
                        CoverageId = coverage.Id,
                        CoverageLimit = 50000m,
                        Deductible = 1000m
                    }
                ]
            },
            CancellationToken.None);

        Assert.AreEqual(409, GetStatusCode(result));
    }
}
