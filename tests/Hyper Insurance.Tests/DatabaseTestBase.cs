using HyperInsurance.Data;
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Sqlite;

namespace Hyper_Insurance.Tests;

public abstract class DatabaseTestBase
{
    private SqliteConnection _connection = null!;

    protected ApplicationDbContext Context { get; private set; } = null!;

    [TestInitialize]
    public async Task InitializeDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new ApplicationDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    [TestCleanup]
    public async Task DisposeDatabase()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    protected async Task<(Customer Customer, Vehicle Vehicle, InsuranceApplication Application)>
        CreateApplicationAsync(
            string status = ApplicationStatuses.Approved,
            int yearsLicensed = 10,
            int accidents = 0,
            int violations = 0,
            decimal vehicleValue = 30000m,
            int horsepower = 150,
            string usage = VehicleUsages.Personal,
            int annualMileage = 12000,
            string storageType = VehicleStorageTypes.Garage)
    {
        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = $"customer{Guid.NewGuid():N}@example.test",
            YearsLicensed = yearsLicensed,
            PreviousAccidents = accidents,
            TrafficViolations = violations
        };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            Make = "Test",
            Model = "Vehicle",
            Year = 2024,
            Value = vehicleValue,
            Horsepower = horsepower,
            Category = "Standard"
        };
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var application = new InsuranceApplication
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            Usage = usage,
            AnnualMileage = annualMileage,
            StorageType = storageType,
            Status = status
        };
        Context.InsuranceApplications.Add(application);
        await Context.SaveChangesAsync();

        return (customer, vehicle, application);
    }

    protected async Task<Coverage> CreateCoverageAsync(
        string name = "Collision",
        bool isActive = true,
        decimal limit = 50000m,
        decimal additionalPremium = 200m,
        decimal deductible = 1000m,
        decimal deductibleAdjustment = -25m)
    {
        var coverage = new Coverage
        {
            Name = name,
            Description = $"{name} coverage",
            IsActive = isActive,
            LimitOptions =
            [
                new CoverageLimitOption
                {
                    CoverageLimit = limit,
                    AdditionalPremium = additionalPremium
                }
            ],
            DeductibleOptions =
            [
                new CoverageDeductibleOption
                {
                    Deductible = deductible,
                    PremiumAdjustment = deductibleAdjustment
                }
            ]
        };
        Context.Coverages.Add(coverage);
        await Context.SaveChangesAsync();
        return coverage;
    }

    protected static void ConfigureController(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    protected static int GetStatusCode<T>(ActionResult<T> result)
    {
        return result.Result switch
        {
            ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            _ => throw new AssertFailedException(
                $"Unexpected action result type: {result.Result?.GetType().Name ?? "null"}.")
        };
    }

    protected static T GetResponseValue<T>(ActionResult<T> result)
        where T : class
    {
        if (result.Result is ObjectResult { Value: T response })
        {
            return response;
        }

        if (result.Value is T directValue)
        {
            return directValue;
        }

        throw new AssertFailedException("The action result did not contain the expected response.");
    }
}
