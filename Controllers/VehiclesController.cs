using HyperInsurance.Data;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly VehicleClassificationService _classificationService;

    public VehiclesController(
        ApplicationDbContext context,
        VehicleClassificationService classificationService)
    {
        _context = context;
        _classificationService = classificationService;
    }

    [HttpPost("classify")]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleResponse>> ClassifyVehicle(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            Value = request.Value,
            Horsepower = request.Horsepower
        };
        vehicle.Category = _classificationService.ClassifyVehicle(vehicle);

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetVehicleById),
            new { id = vehicle.Id },
            ToResponse(vehicle));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> GetVehicles(
        CancellationToken cancellationToken)
    {
        var vehicles = await _context.Vehicles
            .AsNoTracking()
            .OrderBy(vehicle => vehicle.Id)
            .Select(vehicle => new VehicleResponse(
                vehicle.Id,
                vehicle.Make,
                vehicle.Model,
                vehicle.Year,
                vehicle.Value,
                vehicle.Horsepower,
                vehicle.Category))
            .ToListAsync(cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleResponse>> GetVehicleById(
        int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (vehicle is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vehicle not found",
                detail: "No vehicle exists with the requested ID.");
        }

        return Ok(ToResponse(vehicle));
    }

    private static VehicleResponse ToResponse(Vehicle vehicle)
    {
        return new VehicleResponse(
            vehicle.Id,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.Value,
            vehicle.Horsepower,
            vehicle.Category);
    }
}
