using HyperInsurance.Data;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnderwritingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UnderwritingService _underwritingService;

    public UnderwritingController(
        ApplicationDbContext context,
        UnderwritingService underwritingService)
    {
        _context = context;
        _underwritingService = underwritingService;
    }

    [HttpGet("application/{applicationId}")]
    public async Task<ActionResult<UnderwritingResultResponse>> GetApplicationResult(
        int applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _context.UnderwritingResults
            .AsNoTracking()
            .Include(item => item.RiskFactors)
            .FirstOrDefaultAsync(
                item => item.ApplicationId == applicationId,
                cancellationToken);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Underwriting result not found",
                detail: "Evaluate the application before viewing its underwriting result.");
        }

        return Ok(ToResponse(result));
    }

    [HttpPost("evaluate/{applicationId}")]
    public async Task<ActionResult<UnderwritingResultResponse>> EvaluateApplication(
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

        var evaluated = _underwritingService.EvaluateApplication(
            application,
            customer,
            vehicle);

        var result = await _context.UnderwritingResults
            .Include(item => item.RiskFactors)
            .FirstOrDefaultAsync(
                item => item.ApplicationId == applicationId,
                cancellationToken);

        if (result is null)
        {
            result = evaluated;
            _context.UnderwritingResults.Add(result);
        }
        else
        {
            result.Decision = evaluated.Decision;
            result.RiskScore = evaluated.RiskScore;
            result.Reason = evaluated.Reason;
            result.EvaluatedAt = DateTime.UtcNow;
            result.ReviewedBy = null;
            result.ReviewNotes = null;
            result.ReviewedAt = null;
            _context.UnderwritingRiskFactors.RemoveRange(result.RiskFactors);
            result.RiskFactors.Clear();
            foreach (var factor in evaluated.RiskFactors)
            {
                result.RiskFactors.Add(factor);
            }
        }

        application.Status = result.Decision;
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_UnderwritingResults_ApplicationId"
            })
        {
            _context.ChangeTracker.Clear();
            var concurrentlySavedResult = await _context.UnderwritingResults
                .AsNoTracking()
                .Include(item => item.RiskFactors)
                .FirstOrDefaultAsync(
                    item => item.ApplicationId == applicationId,
                    cancellationToken);

            if (concurrentlySavedResult is null)
            {
                throw;
            }

            return Ok(ToResponse(concurrentlySavedResult));
        }

        return Ok(ToResponse(result));
    }

    [HttpPost("review/{applicationId}")]
    public async Task<ActionResult<UnderwritingResultResponse>> ReviewApplication(
        int applicationId,
        [FromBody] ManualReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReviewedBy) ||
            string.IsNullOrWhiteSpace(request.ReviewNotes))
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                [nameof(request.ReviewedBy)] =
                    string.IsNullOrWhiteSpace(request.ReviewedBy)
                        ? ["Reviewer name is required."]
                        : [],
                [nameof(request.ReviewNotes)] =
                    string.IsNullOrWhiteSpace(request.ReviewNotes)
                        ? ["Review notes are required."]
                        : []
                }));
        }

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

        if (application.Status != ApplicationStatuses.Referral)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Application is not referred",
                detail: "Only referred applications can receive manual reviews.");
        }

        var underwritingResult = await _context.UnderwritingResults
            .Include(item => item.RiskFactors)
            .FirstOrDefaultAsync(
                item => item.ApplicationId == applicationId,
                cancellationToken);

        if (underwritingResult is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Underwriting result not found",
                detail: "Evaluate the application before recording a manual review.");
        }

        underwritingResult.Decision = request.Decision;
        underwritingResult.ReviewedBy = request.ReviewedBy.Trim();
        underwritingResult.ReviewNotes = request.ReviewNotes.Trim();
        underwritingResult.ReviewedAt = DateTime.UtcNow;
        application.Status = request.Decision;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(underwritingResult));
    }

    private static UnderwritingResultResponse ToResponse(UnderwritingResult result)
    {
        return new UnderwritingResultResponse(
            result.Id,
            result.ApplicationId,
            result.Decision,
            result.RiskScore,
            result.Reason,
            new DateTimeOffset(result.EvaluatedAt, TimeSpan.Zero),
            result.RiskFactors
                .Select(factor => new UnderwritingRiskFactorResponse(
                    factor.Rule,
                    factor.Explanation,
                    factor.Points))
                .ToList(),
            result.ReviewedBy,
            result.ReviewNotes,
            result.ReviewedAt is null
                ? null
                : new DateTimeOffset(result.ReviewedAt.Value, TimeSpan.Zero));
    }
}
