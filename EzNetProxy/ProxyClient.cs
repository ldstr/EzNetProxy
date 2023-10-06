using System.Net;
using EzNetProxy.Modals;
using EzNetProxy.Exceptions;

namespace EzNetProxy;

public class ProxyClient
{
    public static readonly WebProxy DebugFiddler = CreateProxy(new()
    {
        Type = ProxyType.HTTP,
        Address = "127.0.0.1",
        Port = 8888
    });

    public static WebProxy Parse(string? proxy)
    {
        ArgumentException.ThrowIfNullOrEmpty(
            proxy, nameof(proxy)
            );

        const string prt = "://";

        static BadProxyException Exp() =>
            new("Proxy protocol was not provided!");

        int index = proxy.IndexOf(
            prt,
            StringComparison.Ordinal
            );

        if (index == -1)
            throw Exp();

        ProxyData data = new()
        {
            Type = proxy[..index].ToLower() switch
            {
                "http" => ProxyType.HTTP,
                "socks4" => ProxyType.Socks4,
                "socks4a" => ProxyType.Socks4A,
                "socks5" => ProxyType.Socks5,
                _ => throw new BadProxyException(),
            }
        };

        Helper.ExtractProxyData(
            proxy[(index + prt.Length)..],
            ref data
            );

        return CreateProxy(data);
    }

    public static WebProxy Parse(ProxyType type, string? proxy)
    {
        ProxyData data = new()
        {
            Type = type
        };

        Helper.ExtractProxyData(proxy, ref data);
        return CreateProxy(data);
    }

    private static WebProxy CreateProxy(ProxyData data)
    {
        WebProxy proxy = new()
        {
            Address = Helper.GetAddress(data),
            UseDefaultCredentials = false
        };

        string?
            username = data.Username,
            password = data.Password;

        if (
            !string.IsNullOrEmpty(username)
            && !string.IsNullOrEmpty(password)
            ) proxy.Credentials = new NetworkCredential
            {
                UserName = username,
                Password = password
            };

        return proxy;
    }
}