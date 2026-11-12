using HyperInsurance.Data;
using HyperInsurance.Models;
using HyperInsurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RatingService _ratingService;

    public PoliciesController(
        ApplicationDbContext context,
        RatingService ratingService)
    {
        _context = context;
        _ratingService = ratingService;
    }

    [HttpPost("issue/{applicationId}")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponse>> IssuePolicy(
        int applicationId,
        [FromBody] IssuePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var application = await _context.InsuranceApplications
            .FirstOrDefaultAsync(
                a => a.Id == applicationId,
                cancellationToken);

        if (application == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Application not found",
                detail: "No application exists with the requested ID.");
        }

        if (request?.Coverages is null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Coverages)] = ["Coverage selections are required."]
                }));
        }

        if (request.Coverages.Any(selection => selection is null))
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Coverages)] = ["Coverage selections cannot be null."]
                }));
        }

        var selectedCoverageIds = request.Coverages
            .Select(selection => selection.CoverageId)
            .ToList();

        if (selectedCoverageIds.Count != selectedCoverageIds.Distinct().Count())
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Coverages)] =
                        ["Duplicate coverage selections are not allowed."]
                }));
        }

        var selectedCoverages = await _context.Coverages
            .Where(coverage => selectedCoverageIds.Contains(coverage.Id) && coverage.IsActive)
            .Include(coverage => coverage.LimitOptions)
            .Include(coverage => coverage.DeductibleOptions)
            .ToListAsync(cancellationToken);

        if (selectedCoverages.Count != selectedCoverageIds.Count)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Coverages)] =
                        ["One or more selected coverages are invalid or inactive."]
                }));
        }

        var coverageSelections = new List<PolicyCoverage>();
        foreach (var selection in request.Coverages)
        {
            var coverage = selectedCoverages.Single(
                item => item.Id == selection.CoverageId);
            var limitOption = coverage.LimitOptions.SingleOrDefault(
                option => option.CoverageLimit == selection.CoverageLimit);
            var deductibleOption = coverage.DeductibleOptions.SingleOrDefault(
                option => option.Deductible == selection.Deductible);

            if (limitOption is null || deductibleOption is null)
            {
                return ValidationProblem(new ValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        [nameof(selection.CoverageLimit)] =
                            [$"Selected terms are not configured for coverage {coverage.Id}."]
                    }));
            }

            var deductibleAdjustment = deductibleOption.PremiumAdjustment;
            coverageSelections.Add(new PolicyCoverage
            {
                CoverageId = coverage.Id,
                Coverage = coverage,
                CoverageName = coverage.Name,
                CoverageDescription = coverage.Description,
                CoverageLimit = limitOption.CoverageLimit,
                Deductible = deductibleOption.Deductible,
                LimitPremium = limitOption.AdditionalPremium,
                DeductiblePremiumAdjustment = deductibleAdjustment,
                AdditionalPremium = limitOption.AdditionalPremium + deductibleAdjustment
            });
        }

        if (application.Status != ApplicationStatuses.Approved)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Application is not approved",
                detail: "A policy can only be issued for an approved application.");
        }

        var existingPolicy = await _context.Policies
            .AnyAsync(
                p => p.ApplicationId == applicationId,
                cancellationToken);

        if (existingPolicy)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Policy already issued",
                detail: "A policy has already been issued for this application.");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(
                c => c.Id == application.CustomerId,
                cancellationToken);

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(
                v => v.Id == application.VehicleId,
                cancellationToken);

        if (customer == null || vehicle == null)
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

        var rating = _ratingService.Calculate(
                customer,
                vehicle,
                application,
                coverageSelections
            );

        DateTime startDate = DateTime.UtcNow.Date;
        DateTime expiryDate = startDate.AddYears(1);

        var policy = new Policy
        {
            ApplicationId = application.Id,
            PolicyNumber = $"HIP-{startDate.Year}-{application.Id:D6}",
            AnnualPremium = rating.AnnualPremium,
            Deductible = null,
            StartDate = startDate,
            ExpiryDate = expiryDate,
            Status = PolicyStatuses.Active
        };

        foreach (var policyCoverage in coverageSelections)
        {
            policy.Coverages.Add(policyCoverage);
        }

        _context.Policies.Add(policy);

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetPolicyById),
            new { id = policy.Id },
            ToResponse(policy));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PolicyResponse>>> GetPolicies(
        CancellationToken cancellationToken)
    {
        var policies = await _context.Policies
            .AsNoTracking()
            .Include(policy => policy.Coverages)
            .OrderBy(policy => policy.Id)
            .ToListAsync(cancellationToken);

        return Ok(policies.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyResponse>> GetPolicyById(
        int id,
        CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .AsNoTracking()
            .Include(item => item.Coverages)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (policy == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: "No policy exists with the requested ID.");
        }

        return Ok(ToResponse(policy));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<PolicyOperationResponse>> CancelPolicy(
        int id,
        CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (policy == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: "No policy exists with the requested ID.");
        }

        if (policy.Status != PolicyStatuses.Active)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Policy cannot be cancelled",
                detail: "Only active policies can be cancelled.");
        }

        policy.Status = PolicyStatuses.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new PolicyOperationResponse(
            policy.Id,
            policy.PolicyNumber,
            policy.Status,
            "Policy successfully cancelled."));
    }

    [HttpPost("{id}/renew")]
    public async Task<ActionResult<PolicyResponse>> RenewPolicy(
        int id,
        CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .Include(item => item.Coverages)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (policy == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: "No policy exists with the requested ID.");
        }

        if (policy.Status != PolicyStatuses.Active)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Policy cannot be renewed",
                detail: "Only active policies can be renewed.");
        }

        if (policy.ExpiryDate.Date <= DateTime.UtcNow.Date)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Policy cannot be renewed",
                detail: "Expired policies cannot be renewed.");
        }

        policy.ExpiryDate = policy.ExpiryDate.AddYears(1);

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(policy));
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<PolicyStatusResponse>> GetPolicyStatus(
        int id,
        CancellationToken cancellationToken)
    {
        var policy = await _context.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (policy == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: "No policy exists with the requested ID.");
        }

        string currentStatus = policy.Status;

        if (currentStatus == PolicyStatuses.Active &&
            policy.ExpiryDate.Date <= DateTime.UtcNow.Date)
        {
            currentStatus = PolicyStatuses.Expired;
        }

        return Ok(new PolicyStatusResponse(
            policy.Id,
            policy.PolicyNumber,
            currentStatus,
            new DateTimeOffset(policy.StartDate, TimeSpan.Zero),
            new DateTimeOffset(policy.ExpiryDate, TimeSpan.Zero)));
    }

    private static PolicyResponse ToResponse(Policy policy)
    {
        return new PolicyResponse(
            policy.Id,
            policy.PolicyNumber,
            policy.ApplicationId,
            policy.AnnualPremium,
            "CAD",
            new DateTimeOffset(policy.StartDate, TimeSpan.Zero),
            new DateTimeOffset(policy.ExpiryDate, TimeSpan.Zero),
            policy.Status,
            policy.Coverages
                .OrderBy(coverage => coverage.Id)
                .Select(coverage => new PolicyCoverageResponse(
                    coverage.Id,
                    coverage.PolicyId,
                    coverage.CoverageId,
                    coverage.CoverageName,
                    coverage.CoverageDescription,
                    coverage.CoverageLimit,
                    coverage.Deductible,
                    coverage.LimitPremium,
                    coverage.DeductiblePremiumAdjustment,
                    coverage.AdditionalPremium))
                .ToList());
    }
}