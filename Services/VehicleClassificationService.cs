using HyperInsurance.Models;

namespace HyperInsurance.Services;

public class VehicleClassificationService
{
    public string ClassifyVehicle(Vehicle vehicle)
    {
        if (vehicle.Value >= 1000000)
        {
            return "Hypercar";
        }

        if (vehicle.Value >= 250000)
        {
            return "Supercar";
        }

        if (vehicle.Horsepower >= 500)
        {
            return "High-Performance";
        }

        if (vehicle.Value >= 100000)
        {
            return "Luxury";
        }

        return "Standard";
    }
}