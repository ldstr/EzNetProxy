using System.Net;
using EzNetProxy.Modals;
using EzNetProxy.Exceptions;

namespace EzNetProxy;

public class ProxyClient
{
    public static WebProxy DebugFiddler() => Parse(new ProxyData
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

        if (!proxy.Contains(prt))
            throw Exp();

        string[] split = proxy.Split(
            prt,
            StringSplitOptions.RemoveEmptyEntries
            );

        if (split.Length != 2)
            throw Exp();

        ProxyData data = new()
        {
            Type = split[0].ToLower() switch
            {
                "http" => ProxyType.HTTP,
                "socks4" => ProxyType.Socks4,
                "socks4a" => ProxyType.Socks4A,
                "socks5" => ProxyType.Socks5,
                _ => throw new NotImplementedException("Unsupported protocol!"),
            }
        };

        Helper.ExtractProxyData(split[1], ref data);
        return Parse(data);
    }

    public static WebProxy Parse(ProxyType type, string? proxy)
    {
        ProxyData data = new()
        {
            Type = type
        };

        Helper.ExtractProxyData(proxy, ref data);
        return Parse(data);
    }

    public static WebProxy Parse(ProxyData data)
    {
        WebProxy proxy = new()
        {
            Address = Helper.GetAddress(data),
            UseDefaultCredentials = false
        };

        string?
            user = data.Username,
            pw = data.Password;

        if (
            string.IsNullOrEmpty(user) ||
            string.IsNullOrEmpty(pw)
            ) return proxy;

        proxy.Credentials = new NetworkCredential
        {
            UserName = user,
            Password = pw
        };

        return proxy;
    }
}