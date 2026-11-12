using HyperInsurance.Data;
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InsuranceApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InsuranceApplicationResponse>> CreateApplication(
        [FromBody] CreateInsuranceApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var customerExists = await _context.Customers
            .AnyAsync(customer => customer.Id == request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Customer not found",
                detail: "No customer exists with the requested ID.");
        }

        var vehicleExists = await _context.Vehicles
            .AnyAsync(vehicle => vehicle.Id == request.VehicleId, cancellationToken);
        if (!vehicleExists)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vehicle not found",
                detail: "No vehicle exists with the requested ID.");
        }

        var application = new InsuranceApplication
        {
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            Usage = request.Usage.Trim(),
            AnnualMileage = request.AnnualMileage,
            StorageType = request.StorageType.Trim(),
            Status = ApplicationStatuses.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.InsuranceApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.InsuranceApplications
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Vehicle)
            .FirstAsync(item => item.Id == application.Id, cancellationToken);

        return CreatedAtAction(
            nameof(GetApplicationById),
            new { id = application.Id },
            ToResponse(created));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InsuranceApplicationResponse>>> GetApplications(
        CancellationToken cancellationToken)
    {
        var applications = await _context.InsuranceApplications
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Vehicle)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return Ok(applications.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InsuranceApplicationResponse>> GetApplicationById(
        int id,
        CancellationToken cancellationToken)
    {
        var application = await _context.InsuranceApplications
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Vehicle)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (application is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Application not found",
                detail: "No application exists with the requested ID.");
        }

        return Ok(ToResponse(application));
    }

    private static InsuranceApplicationResponse ToResponse(InsuranceApplication application)
    {
        return new InsuranceApplicationResponse(
            application.Id,
            application.CustomerId,
            new ApplicationCustomerResponse(
                application.Customer!.Id,
                application.Customer.FirstName,
                application.Customer.LastName,
                application.Customer.Email),
            application.VehicleId,
            new ApplicationVehicleResponse(
                application.Vehicle!.Id,
                application.Vehicle.Make,
                application.Vehicle.Model,
                application.Vehicle.Year,
                application.Vehicle.Value,
                application.Vehicle.Horsepower,
                application.Vehicle.Category),
            application.Usage,
            application.AnnualMileage,
            application.StorageType,
            application.Status,
            new DateTimeOffset(application.SubmittedAt, TimeSpan.Zero));
    }
}
