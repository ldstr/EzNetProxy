using EzNetProxy.Modals;
using EzNetProxy.Exceptions;

namespace EzNetProxy;

internal class Helper
{
    public static Uri GetAddress(ProxyData data)
    {
        string typeStr = data.Type switch
        {
            ProxyType.HTTP => "http",
            ProxyType.Socks4 => "socks4",
            ProxyType.Socks4A => "socks4a",
            ProxyType.Socks5 => "socks5",
            _ => throw new BadProxyException()
        };

        return new($"{typeStr}://{data.Address}:{data.Port}");
    }

    public static void ExtractProxyData(
        string? input, ref ProxyData data
        )
    {
        ArgumentException.ThrowIfNullOrEmpty(
            input, nameof(input)
            );

        const char sp = '@';

        if (input.Contains(sp))
        {
            string[] split = input.Split(
                sp, SPLIT_OPTS
                );

            string[]
                creds = SplitStr(split[0]),
                proxy = SplitStr(split[1]);

            TryGetProxyData(proxy, ref data);

            data.Username = creds[0];
            data.Password = creds[1];
        }
        else
        {
            string[] split = SplitStr(input);
            TryGetProxyData(split, ref data);

            if (split.Length >= 4)
            {
                data.Username = split[2];
                data.Password = split[3];
            }
        }
    }

    private static void TryGetProxyData(
        string[] input,
        ref ProxyData data
        )
    {
        if (input.Length < 2)
            throw new BadProxyException();

        data.Address = input[0];
        data.Port = TryConvPort(input[1]);
    }

    private static int TryConvPort(string port)
    {
        if (int.TryParse(port, out int result))
            return result;

        throw new BadProxyException();
    }

    private static string[] SplitStr(string str) => str.Split(
        _splitters,
        SPLIT_OPTS
        );

    private static readonly char[] _splitters = { ':', ';', '|' };

    private const StringSplitOptions SPLIT_OPTS =
        StringSplitOptions.RemoveEmptyEntries
        | StringSplitOptions.TrimEntries;
}