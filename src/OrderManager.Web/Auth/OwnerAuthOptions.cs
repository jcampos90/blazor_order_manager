namespace OrderManager.Web.Auth;

public sealed record OwnerAuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// When on (development default), the first successful sign-in is persisted as the
    /// owner. Disable for production, where the owner must already be recorded.
    /// </summary>
    public bool AllowClaim { get; init; } = true;
}