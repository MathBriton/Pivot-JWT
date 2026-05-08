namespace AuthJWT.Api.Models;

public static class AuditAction
{
    public const string AuthLogin         = "Auth.Login";
    public const string AuthLoginFailed   = "Auth.LoginFailed";
    public const string AuthRegister      = "Auth.Register";
    public const string AuthLogout        = "Auth.Logout";
    public const string AuthRefreshToken  = "Auth.RefreshToken";

    public const string CustomerCreated   = "Customer.Created";
    public const string CustomerUpdated   = "Customer.Updated";
    public const string CustomerDeleted   = "Customer.Deleted";

    public const string WorkTaskCreated       = "WorkTask.Created";
    public const string WorkTaskUpdated       = "WorkTask.Updated";
    public const string WorkTaskStatusChanged = "WorkTask.StatusChanged";
}
