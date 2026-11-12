using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hyper_Insurance.Tests;

[TestClass]
public sealed class VehicleClassificationTests
{
    private readonly VehicleClassificationService _classifier = new();

    [DataTestMethod]
    [DataRow(30000, 150, "Standard")]
    [DataRow(100000, 150, "Luxury")]
    [DataRow(150000, 550, "High-Performance")]
    [DataRow(250000, 150, "Supercar")]
    [DataRow(1000000, 150, "Hypercar")]
    public void ClassifiesVehicleFromValueAndPerformance(
        int value,
        int horsepower,
        string expectedCategory)
    {
        var vehicle = new Vehicle
        {
            Value = value,
            Horsepower = horsepower
        };

        var category = _classifier.ClassifyVehicle(vehicle);

        Assert.AreEqual(expectedCategory, category);
    }
}
