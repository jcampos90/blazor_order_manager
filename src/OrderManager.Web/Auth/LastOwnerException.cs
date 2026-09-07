namespace OrderManager.Web.Auth;

public sealed class LastOwnerException : Exception
{
    public LastOwnerException(string message) : base(message) { }
}
