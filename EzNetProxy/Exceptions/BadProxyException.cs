namespace EzNetProxy.Exceptions;

internal class BadProxyException : Exception
{
    public BadProxyException() : base("Invalid proxy!") { }

    public BadProxyException(string message) : base(message) { }
}