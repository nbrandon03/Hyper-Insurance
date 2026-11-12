using System.ComponentModel.DataAnnotations;

namespace HyperInsurance.Models;

/// <summary>Creates a customer profile.</summary>
public sealed record CreateCustomerRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string LastName { get; init; }

    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [Range(0, 100)]
    public int YearsLicensed { get; init; }

    [Range(0, int.MaxValue)]
    public int PreviousAccidents { get; init; }

    [Range(0, int.MaxValue)]
    public int TrafficViolations { get; init; }
}

/// <summary>Customer information returned by the API.</summary>
public sealed record CustomerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    int YearsLicensed,
    int PreviousAccidents,
    int TrafficViolations);

/// <summary>Creates a vehicle that can be insured.</summary>
public sealed record CreateVehicleRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Make { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Model { get; init; }

    [Range(1886, 2200)]
    public int Year { get; init; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Value { get; init; }

    [Range(0, 10000)]
    public int Horsepower { get; init; }
}

/// <summary>Vehicle information returned by the API.</summary>
public sealed record VehicleResponse(
    int Id,
    string Make,
    string Model,
    int Year,
    decimal Value,
    int Horsepower,
    string Category);

/// <summary>Submits an insurance application for a customer and vehicle.</summary>
public sealed record CreateInsuranceApplicationRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; init; }

    [Required, StringLength(50, MinimumLength = 1)]
    public required string Usage { get; init; }

    [Range(0, int.MaxValue)]
    public int AnnualMileage { get; init; }

    [Required, StringLength(50, MinimumLength = 1)]
    public required string StorageType { get; init; }
}

/// <summary>Compact customer details associated with an application.</summary>
public sealed record ApplicationCustomerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email);

/// <summary>Compact vehicle details associated with an application.</summary>
public sealed record ApplicationVehicleResponse(
    int Id,
    string Make,
    string Model,
    int Year,
    decimal Value,
    int Horsepower,
    string Category);

/// <summary>Insurance application details returned by the API.</summary>
public sealed record InsuranceApplicationResponse(
    int Id,
    int CustomerId,
    ApplicationCustomerResponse Customer,
    int VehicleId,
    ApplicationVehicleResponse Vehicle,
    string Usage,
    int AnnualMileage,
    string StorageType,
    string Status,
    DateTimeOffset SubmittedAt);

/// <summary>Returns the service's current health and identity.</summary>
public sealed record InsuranceHealthResponse(
    string Company,
    string Message,
    string Description,
    string Status);

/// <summary>Coverage terms attached to an issued policy.</summary>
public sealed record PolicyCoverageResponse(
    int Id,
    int PolicyId,
    int CoverageId,
    string Name,
    string Description,
    decimal CoverageLimit,
    decimal Deductible,
    decimal LimitPremium,
    decimal DeductiblePremiumAdjustment,
    decimal AdditionalPremium);

/// <summary>Policy information with its selected coverage terms.</summary>
public sealed record PolicyResponse(
    int Id,
    string PolicyNumber,
    int ApplicationId,
    decimal AnnualPremium,
    string Currency,
    DateTimeOffset StartDate,
    DateTimeOffset ExpiryDate,
    string Status,
    IReadOnlyList<PolicyCoverageResponse> Coverages);

/// <summary>Individual factor contributing to an underwriting risk score.</summary>
public sealed record UnderwritingRiskFactorResponse(
    string Rule,
    string Explanation,
    decimal Points);

/// <summary>Underwriting decision and its risk-score explanation.</summary>
public sealed record UnderwritingResultResponse(
    int Id,
    int ApplicationId,
    string Decision,
    decimal RiskScore,
    string Reason,
    DateTimeOffset EvaluatedAt,
    IReadOnlyList<UnderwritingRiskFactorResponse> RiskFactors,
    string? ReviewedBy,
    string? ReviewNotes,
    DateTimeOffset? ReviewedAt);

/// <summary>One component contributing to an annual premium.</summary>
public sealed record PremiumComponentResponse(string Name, decimal Amount);

/// <summary>Annual premium quote and its component breakdown.</summary>
public sealed record RatingQuoteResponse(
    int ApplicationId,
    string Vehicle,
    string Currency,
    IReadOnlyList<PremiumComponentResponse> Breakdown,
    decimal AnnualPremium,
    string ApplicationStatus);

/// <summary>Returns the outcome of an operation on a policy.</summary>
public sealed record PolicyOperationResponse(
    int Id,
    string PolicyNumber,
    string Status,
    string Message);

/// <summary>Current policy status and effective dates.</summary>
public sealed record PolicyStatusResponse(
    int Id,
    string PolicyNumber,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset ExpiryDate);

/// <summary>Returns a coverage product's activation state.</summary>
public sealed record CoverageActivationResponse(int Id, bool IsActive);
