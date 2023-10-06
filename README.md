# EzNetProxy

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![dotnet6.0](https://img.shields.io/badge/.NET-7.0-blue?style=for-the-badge)
![License-MIT](https://img.shields.io/badge/License-MIT-Green?style=for-the-badge)

A library to simplify proxy usage in .NET 7+

---

### Usage
1. Install package via [Nuget](https://www.nuget.org/packages/EzNetProxy) or some other method 
2. Import modules: `using EzNetProxy;`
3. Use your preferred way to parse the proxy connection information.

### Notes:
- Requires .NET 7+
- Backbone connections supported
- ipv6 works (backbone connection)

### Supported proxy formats:
- `host:port`
- `host:port:user:pw`
- `user:pw@host:port`

---

Method #1:
```csharp
var httpProxy = ProxyClient.Parse("http://host:port");
var socks4Proxy = ProxyClient.Parse("socks4://host:port:user:pw");
var socks4aProxy = ProxyClient.Parse("socks4a://user:pw@host:port");
var socks5Proxy = ProxyClient.Parse("socks5://host:port");
```

Method #2:
```csharp
string proxy = "123.123.123.123:8080";
var httpProxy = ProxyClient.Parse(ProxyType.HTTP, proxy);
...
```

Method #3:
```csharp
using EzNetProxy.Modals;

ProxyData data = new()
{
    Type = ProxyType.HTTP,
    Ip = "pornhub.com",
    Port = 80,
    Username = "username",
    Password = "password"
};

var proxy = ProxyClient.Parse(data);
```

You can also use Fiddler's debugging proxy by:
```csharp
var proxy = ProxyClient.DebugFiddler();
```

---

Here is a simple example:

```csharp
// backbone connection; ipv4; 104.227.XXX.XXX
var proxy = ProxyClient.Parse(ProxyType.HTTP, "q.webshare.io:80");

HttpClientHandler httpClientHandler = new()
{
  Proxy = proxy,
  UseProxy = true // IMPORTANT!
};

using HttpClient client = new(httpClientHandler, true);

var str = await client.GetStringAsync("https://wtfismyip.com/text");

Console.WriteLine(str);
// str = 104.227.XXX.XXX

```

---

This project is inspired by [xNet](https://github.com/X-rus/xNet); the project is no longer maintained by its creator and even many forks of the project, such as [Leaf.xNet](https://github.com/csharp-leaf/Leaf.xNet). One of the main reasons why xNet was treasured was for its support of many proxy protocols, such as Socks-4 and Socks-5, and how simple it was to use. Hopefully, this project can be an excellent replacement for one part of the project — simple proxy usage.

Please check my profile if you'd like to contact me.
