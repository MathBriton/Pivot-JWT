namespace AuthJWT.Api.Models;

public static class WorkTaskStatus
{
    public const string Pending    = "Pending";
    public const string InProgress = "InProgress";
    public const string Review     = "Review";
    public const string Completed  = "Completed";
    public const string Cancelled  = "Cancelled";

    public static readonly IReadOnlyCollection<string> All =
        [Pending, InProgress, Review, Completed, Cancelled];
}
