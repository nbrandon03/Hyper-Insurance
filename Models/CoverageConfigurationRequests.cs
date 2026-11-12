using System.ComponentModel.DataAnnotations;

namespace HyperInsurance.Models;

/// <summary>Defines a coverage limit and its additional premium.</summary>
public sealed record CoverageLimitOptionRequest
{
    [Range(typeof(decimal), "0.01", "999999999999")]
    public required decimal CoverageLimit { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public required decimal AdditionalPremium { get; init; }
}

/// <summary>Defines a deductible and its signed premium adjustment; negative values reduce premium.</summary>
public sealed record CoverageDeductibleOptionRequest
{
    [Range(typeof(decimal), "0", "999999999999")]
    public required decimal Deductible { get; init; }

    [Range(typeof(decimal), "-999999999999", "999999999999")]
    public required decimal PremiumAdjustment { get; init; }
}

/// <summary>Creates or replaces a configurable coverage product.</summary>
public sealed record SaveCoverageConfigurationRequest
{
    [Required, StringLength(150, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(2000)]
    public required string Description { get; init; }

    public required bool IsActive { get; init; }

    [Required, MinLength(1), MaxLength(50)]
    public required List<CoverageLimitOptionRequest> LimitOptions { get; init; }

    [Required, MinLength(1), MaxLength(50)]
    public required List<CoverageDeductibleOptionRequest> DeductibleOptions { get; init; }
}

/// <summary>Describes a selectable coverage limit and its premium.</summary>
public sealed record CoverageLimitOptionResponse(
    decimal CoverageLimit,
    decimal AdditionalPremium);

/// <summary>Describes a selectable deductible and its premium adjustment.</summary>
public sealed record CoverageDeductibleOptionResponse(
    decimal Deductible,
    decimal PremiumAdjustment);

/// <summary>Represents a coverage and its current product configuration.</summary>
public sealed record CoverageConfigurationResponse(
    int Id,
    string Name,
    string Description,
    bool IsActive,
    IReadOnlyList<CoverageLimitOptionResponse> LimitOptions,
    IReadOnlyList<CoverageDeductibleOptionResponse> DeductibleOptions);

/// <summary>Sets whether customers can select a coverage option.</summary>
public sealed record SetCoverageActivationRequest
{
    public required bool IsActive { get; init; }
}

/// <summary>Specifies the selected terms when adding a coverage to a policy.</summary>
public sealed record SelectCoverageTermsRequest
{
    [Range(typeof(decimal), "0.01", "999999999999")]
    public required decimal CoverageLimit { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public required decimal Deductible { get; init; }
}
