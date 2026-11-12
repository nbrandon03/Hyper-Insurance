using HyperInsurance.Models;

namespace HyperInsurance.Services;

public sealed class UnderwritingService(IEnumerable<IUnderwritingRule> rules)
{
    public UnderwritingResult EvaluateApplication(
        InsuranceApplication application,
        Customer customer,
        Vehicle vehicle)
    {
        var context = new UnderwritingEvaluationContext(
            application,
            customer,
            vehicle);
        var factors = rules
            .SelectMany(rule => rule.Evaluate(context))
            .ToList();
        var riskScore = factors.Sum(factor => factor.Points);
        var requiresTrackReview = factors.Any(factor =>
            factor.Rule == "Vehicle usage");

        var decision = requiresTrackReview
            ? UnderwritingDecisions.Referral
            : riskScore switch
            {
                >= 70 => UnderwritingDecisions.Declined,
                >= 40 => UnderwritingDecisions.Referral,
                _ => UnderwritingDecisions.Approved
            };

        var reason = factors.Count == 0
            ? "No scored risk factors were identified; application meets standard underwriting criteria."
            : $"Risk score {riskScore:0.##}. " +
              string.Join(" ", factors.Select(factor =>
                  $"{factor.Explanation} (+{factor.Points:0.##} points)."));

        if (requiresTrackReview)
        {
            reason += " Specialist review is required for track usage.";
        }
        else if (decision == UnderwritingDecisions.Declined)
        {
            reason += " The score meets or exceeds the decline threshold of 70.";
        }
        else if (decision == UnderwritingDecisions.Referral)
        {
            reason += " The score meets or exceeds the referral threshold of 40.";
        }

        return new UnderwritingResult
        {
            ApplicationId = application.Id,
            Decision = decision,
            RiskScore = riskScore,
            Reason = reason,
            RiskFactors = factors
        };
    }
}
