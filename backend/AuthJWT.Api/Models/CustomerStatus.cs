namespace AuthJWT.Api.Models;

public static class CustomerStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly IReadOnlyCollection<string> All = [Active, Inactive];
}
