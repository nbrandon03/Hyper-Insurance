using HyperInsurance.Data;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RatingService _ratingService;

    public RatingController(
        ApplicationDbContext context,
        RatingService ratingService)
    {
        _context = context;
        _ratingService = ratingService;
    }

    [HttpGet("quote/{applicationId}")]
    public async Task<ActionResult<RatingQuoteResponse>> CalculateQuote(
        int applicationId,
        CancellationToken cancellationToken)
    {
        var application = await _context.InsuranceApplications
            .FirstOrDefaultAsync(
                item => item.Id == applicationId,
                cancellationToken);

        if (application is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Application not found",
                detail: "No application exists with the requested ID.");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(
                item => item.Id == application.CustomerId,
                cancellationToken);
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(
                item => item.Id == application.VehicleId,
                cancellationToken);

        if (customer is null || vehicle is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Application data not found",
                detail: "The application is missing its customer or vehicle.");
        }

        var validationErrors = InsuranceInputValidator.Validate(
            application,
            customer,
            vehicle);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(
                new ValidationProblemDetails(validationErrors.ToDictionary()));
        }

        var calculation = _ratingService.Calculate(
            customer,
            vehicle,
            application);

        return Ok(new RatingQuoteResponse(
            application.Id,
            $"{vehicle.Make} {vehicle.Model}",
            "CAD",
            calculation.Breakdown
                .Select(component => new PremiumComponentResponse(
                    component.Name,
                    component.Amount))
                .ToList(),
            calculation.AnnualPremium,
            application.Status));
    }
}
