using HyperInsurance.Data;
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolicyCoveragesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PolicyCoveragesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all coverages attached to a policy
    [HttpGet("policy/{policyId}")]
    public async Task<ActionResult<IReadOnlyList<PolicyCoverageResponse>>> GetPolicyCoverages(
        int policyId,
        CancellationToken cancellationToken)
    {
        var policyExists = await _context.Policies
            .AnyAsync(p => p.Id == policyId, cancellationToken);

        if (!policyExists)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Policy not found",
                detail: "No policy exists with the requested ID.");
        }

        var coverages = await _context.PolicyCoverages
            .AsNoTracking()
            .Where(pc => pc.PolicyId == policyId)
            .OrderBy(pc => pc.Id)
            .ToListAsync(cancellationToken);

        return Ok(coverages.Select(pc => new PolicyCoverageResponse(
            pc.Id,
            pc.PolicyId,
            pc.CoverageId,
            pc.CoverageName,
            pc.CoverageDescription,
            pc.CoverageLimit,
            pc.Deductible,
            pc.LimitPremium,
            pc.DeductiblePremiumAdjustment,
            pc.AdditionalPremium)).ToList());
    }

    // Add a coverage to a policy
    [HttpPost("policy/{policyId}/coverage/{coverageId}")]
    public async Task<ActionResult<PolicyCoverageResponse>> AddCoverage(
        int policyId,
        int coverageId,
        [FromBody] SelectCoverageTermsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CoverageLimit <= 0 || request.Deductible < 0)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.CoverageLimit)] =
                        request.CoverageLimit <= 0
                            ? ["Coverage limit must be positive."]
                            : [],
                    [nameof(request.Deductible)] =
                        request.Deductible < 0
                            ? ["Deductible cannot be negative."]
                            : []
                }));
        }

        var policy = await _context.Policies
            .FirstOrDefaultAsync(item => item.Id == policyId, cancellationToken);

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
                title: "Policy cannot be changed",
                detail: "Coverage can only be added to active policies.");
        }

        var coverage = await _context.Coverages
            .Include(item => item.LimitOptions)
            .Include(item => item.DeductibleOptions)
            .FirstOrDefaultAsync(item => item.Id == coverageId, cancellationToken);

        if (coverage == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Coverage not found",
                detail: "No coverage exists with the requested ID.");
        }

        if (!coverage.IsActive)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Coverage is inactive",
                detail: "Inactive coverage cannot be added to a policy.");
        }

        var limitOption = coverage.LimitOptions
            .SingleOrDefault(option => option.CoverageLimit == request.CoverageLimit);
        var deductibleOption = coverage.DeductibleOptions
            .SingleOrDefault(option => option.Deductible == request.Deductible);

        if (limitOption is null || deductibleOption is null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.CoverageLimit)] =
                        limitOption is null
                            ? ["The selected limit is not configured for this coverage."]
                            : [],
                    [nameof(request.Deductible)] =
                        deductibleOption is null
                            ? ["The selected deductible is not configured for this coverage."]
                            : []
                }));
        }

        var alreadyAdded = await _context.PolicyCoverages
            .AnyAsync(pc =>
                pc.PolicyId == policyId &&
                pc.CoverageId == coverageId,
                cancellationToken);

        if (alreadyAdded)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Coverage already selected",
                detail: "This coverage is already attached to the policy.");
        }

        var policyCoverage = new PolicyCoverage
        {
            PolicyId = policyId,
            CoverageId = coverageId,
            CoverageName = coverage.Name,
            CoverageDescription = coverage.Description,
            CoverageLimit = limitOption.CoverageLimit,
            Deductible = deductibleOption.Deductible,
            LimitPremium = limitOption.AdditionalPremium,
            DeductiblePremiumAdjustment = deductibleOption.PremiumAdjustment,
            AdditionalPremium = limitOption.AdditionalPremium + deductibleOption.PremiumAdjustment
        };

        _context.PolicyCoverages.Add(policyCoverage);
        policy.AnnualPremium = Math.Round(
            policy.AnnualPremium + policyCoverage.AdditionalPremium,
            2);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new PolicyCoverageResponse(
            policyCoverage.Id,
            policyCoverage.PolicyId,
            policyCoverage.CoverageId,
            policyCoverage.CoverageName,
            policyCoverage.CoverageDescription,
            policyCoverage.CoverageLimit,
            policyCoverage.Deductible,
            policyCoverage.LimitPremium,
            policyCoverage.DeductiblePremiumAdjustment,
            policyCoverage.AdditionalPremium));
    }
}