using HyperInsurance.Data;
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoveragesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoveragesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CoverageConfigurationResponse>>> GetCoverages(
        CancellationToken cancellationToken)
    {
        var coverages = await _context.Coverages
            .Where(coverage => coverage.IsActive)
            .Include(coverage => coverage.LimitOptions)
            .Include(coverage => coverage.DeductibleOptions)
            .OrderBy(coverage => coverage.Name)
            .ToListAsync(cancellationToken);

        return Ok(coverages.Select(ToResponse).ToList());
    }

    [HttpGet("configuration")]
    public async Task<ActionResult<IReadOnlyList<CoverageConfigurationResponse>>> GetCoverageConfigurations(
        CancellationToken cancellationToken)
    {
        var coverages = await _context.Coverages
            .Include(coverage => coverage.LimitOptions)
            .Include(coverage => coverage.DeductibleOptions)
            .OrderBy(coverage => coverage.Name)
            .ToListAsync(cancellationToken);

        return Ok(coverages.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CoverageConfigurationResponse>> GetCoverage(
        int id,
        CancellationToken cancellationToken)
    {
        var coverage = await _context.Coverages
            .Include(item => item.LimitOptions)
            .Include(item => item.DeductibleOptions)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (coverage is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Coverage not found",
                detail: "No coverage exists with the requested ID.");
        }

        return Ok(ToResponse(coverage));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CoverageConfigurationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoverageConfigurationResponse>> CreateCoverage(
        [FromBody] SaveCoverageConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["configuration"] = [validationError]
                }));
        }

        var coverage = CreateCoverageEntity(request);
        _context.Coverages.Add(coverage);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetCoverage),
            new { id = coverage.Id },
            ToResponse(coverage));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CoverageConfigurationResponse>> UpdateCoverage(
        int id,
        [FromBody] SaveCoverageConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["configuration"] = [validationError]
                }));
        }

        var coverage = await _context.Coverages
            .Include(item => item.LimitOptions)
            .Include(item => item.DeductibleOptions)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (coverage is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Coverage not found",
                detail: "No coverage exists with the requested ID.");
        }

        coverage.Name = request.Name.Trim();
        coverage.Description = request.Description.Trim();
        coverage.IsActive = request.IsActive;

        _context.CoverageLimitOptions.RemoveRange(coverage.LimitOptions);
        coverage.LimitOptions.Clear();
        foreach (var option in request.LimitOptions)
        {
            coverage.LimitOptions.Add(new CoverageLimitOption
            {
                CoverageLimit = option.CoverageLimit,
                AdditionalPremium = option.AdditionalPremium
            });
        }

        _context.CoverageDeductibleOptions.RemoveRange(coverage.DeductibleOptions);
        coverage.DeductibleOptions.Clear();
        foreach (var option in request.DeductibleOptions)
        {
            coverage.DeductibleOptions.Add(new CoverageDeductibleOption
            {
                Deductible = option.Deductible,
                PremiumAdjustment = option.PremiumAdjustment
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(coverage));
    }

    [HttpPatch("{id}/activation")]
    public async Task<ActionResult<CoverageActivationResponse>> SetCoverageActivation(
        int id,
        [FromBody] SetCoverageActivationRequest request,
        CancellationToken cancellationToken)
    {
        var coverage = await _context.Coverages
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (coverage is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Coverage not found",
                detail: "No coverage exists with the requested ID.");
        }

        coverage.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new CoverageActivationResponse(coverage.Id, coverage.IsActive));
    }

    private static Coverage CreateCoverageEntity(
        SaveCoverageConfigurationRequest request)
    {
        var coverage = new Coverage
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IsActive = request.IsActive
        };

        foreach (var option in request.LimitOptions)
        {
            coverage.LimitOptions.Add(new CoverageLimitOption
            {
                CoverageLimit = option.CoverageLimit,
                AdditionalPremium = option.AdditionalPremium
            });
        }

        foreach (var option in request.DeductibleOptions)
        {
            coverage.DeductibleOptions.Add(new CoverageDeductibleOption
            {
                Deductible = option.Deductible,
                PremiumAdjustment = option.PremiumAdjustment
            });
        }

        return coverage;
    }

    private static string? Validate(SaveCoverageConfigurationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Coverage name is required.";
        }

        if (request.Description is null)
        {
            return "Coverage description cannot be null.";
        }

        if (request.LimitOptions is null || request.LimitOptions.Count == 0)
        {
            return "At least one coverage limit option is required.";
        }

        if (request.DeductibleOptions is null || request.DeductibleOptions.Count == 0)
        {
            return "At least one deductible option is required.";
        }

        if (request.LimitOptions.Any(option =>
                option is null ||
                option.CoverageLimit <= 0 ||
                option.AdditionalPremium < 0))
        {
            return "Coverage limits must be positive and their additional premiums cannot be negative.";
        }

        if (request.LimitOptions
            .Select(option => option.CoverageLimit)
            .Distinct()
            .Count() != request.LimitOptions.Count)
        {
            return "Coverage limit options cannot contain duplicate amounts.";
        }

        if (request.DeductibleOptions.Any(option =>
                option is null || option.Deductible < 0))
        {
            return "Deductible amounts cannot be negative.";
        }

        if (request.DeductibleOptions
            .Select(option => option.Deductible)
            .Distinct()
            .Count() != request.DeductibleOptions.Count)
        {
            return "Deductible options cannot contain duplicate amounts.";
        }

        return null;
    }

    private static CoverageConfigurationResponse ToResponse(Coverage coverage)
    {
        return new CoverageConfigurationResponse(
            coverage.Id,
            coverage.Name,
            coverage.Description,
            coverage.IsActive,
            coverage.LimitOptions
                .OrderBy(option => option.CoverageLimit)
                .Select(option => new CoverageLimitOptionResponse(
                    option.CoverageLimit,
                    option.AdditionalPremium))
                .ToList(),
            coverage.DeductibleOptions
                .OrderBy(option => option.Deductible)
                .Select(option => new CoverageDeductibleOptionResponse(
                    option.Deductible,
                    option.PremiumAdjustment))
                .ToList());
    }
}
