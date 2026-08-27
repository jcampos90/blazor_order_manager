namespace OrderManager.Web.Auth;

public sealed record ClerkAuthOptions
{
    public const string SectionName = "Auth:Oidc";

    public string Authority { get; init; } = "";
    public string ClientId { get; init; } = "";
    public string ClientSecret { get; init; } = "";
    /// <summary>Stored per the auth ticket; not consumed by the OIDC middleware.</summary>
    public string PublishableKey { get; init; } = "";
}