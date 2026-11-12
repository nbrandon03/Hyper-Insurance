namespace HyperInsurance.Models;

/// <summary>One component contributing to an annual premium.</summary>
public sealed record PremiumComponent(string Name, decimal Amount);

/// <summary>A complete annual premium calculation and its component breakdown.</summary>
public sealed record RatingCalculation(
    decimal AnnualPremium,
    IReadOnlyList<PremiumComponent> Breakdown);
