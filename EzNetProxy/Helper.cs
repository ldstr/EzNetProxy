using EzNetProxy.Modals;
using EzNetProxy.Exceptions;

namespace EzNetProxy
{
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

            return new Uri($"{typeStr}://{data.Ip}:{data.Port}");
        }

        public static void ExtractProxyData(string? input, ref ProxyData data)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            /*
             * ip:port
             * ip:port:user:pw
             * user:pw@ip:port
             */

            const char sp = '@';

            if (input.Contains(sp))
            {
                string[] split = input.Split(sp, SplitRemOp);

                CheckValidity(split);

                string[]
                    creds = SplitStr(split[0]),
                    proxy = SplitStr(split[1]);

                TryGetProxyData(proxy, ref data);
                CheckValidity(creds);

                data.Username = creds[0];
                data.Password = creds[1];
            }
            else
            {
                string[] split = SplitStr(input);

                CheckValidity(split);
                TryGetProxyData(split, ref data);

                if (split.Length >= 4)
                {
                    data.Username = split[2];
                    data.Password = split[3];
                }
            }
        }

        private static void CheckValidity(string[] input)
        {
            if (input.Length < 2)
                throw new BadProxyException();
        }

        private static void TryGetProxyData(string[] input, ref ProxyData data)
        {
            if (input.Length < 2)
                throw new BadProxyException();

            data.Ip = input[0];
            data.Port = TryConvPort(input[1]);
        }

        private static int TryConvPort(string port)
        {
            if (int.TryParse(port, out int result))
                return result;

            throw new BadProxyException();
        }

        private static string[] SplitStr(string str)
        {
            char[] splitters = {
                ':',
                ';',
                '|'
            };

            return str.Split(splitters, SplitRemOp);
        }

        private static readonly StringSplitOptions
            SplitRemOp = StringSplitOptions.RemoveEmptyEntries;
    }
}
