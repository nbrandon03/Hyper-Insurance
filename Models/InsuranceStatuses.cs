namespace HyperInsurance.Models;

public static class ApplicationStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Declined = "Declined";
    public const string Referral = "Referral";
    public const string CheckConstraintSql =
        "\"Status\" IN ('" + Pending + "', '" + Approved + "', '" + Declined + "', '" + Referral + "')";
}

public static class UnderwritingDecisions
{
    public const string Pending = ApplicationStatuses.Pending;
    public const string Approved = ApplicationStatuses.Approved;
    public const string Declined = ApplicationStatuses.Declined;
    public const string Referral = ApplicationStatuses.Referral;
    public const string ManualReviewDecisionPattern =
        "^(" + Approved + "|" + Declined + ")$";
    public const string CheckConstraintSql =
        "\"Decision\" IN ('" + Pending + "', '" + Approved + "', '" + Declined + "', '" + Referral + "')";
}

public static class PolicyStatuses
{
    public const string Active = "Active";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
    public const string CheckConstraintSql =
        "\"Status\" IN ('" + Active + "', '" + Cancelled + "')";
}

public static class VehicleUsages
{
    public const string Personal = "Personal";
    public const string Business = "Business";
    public const string Track = "Track";
}

public static class VehicleStorageTypes
{
    public const string Garage = "Garage";
    public const string Outdoor = "Outdoor";
}
