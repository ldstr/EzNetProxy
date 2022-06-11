# EzNetProxy

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![dotnet6.0](https://img.shields.io/badge/.NET-6.0-blue?style=for-the-badge)
![License-MIT](https://img.shields.io/badge/License-MIT-Green?style=for-the-badge)

A library to simplify proxy usage in .NET 6+

---

### Usage
1. Install package via [Nuget](https://www.nuget.org/packages/EzNetProxy) or some other method 
2. Import modules: `using EzNetProxy;`
3. Use your preferred way to parse the proxy connection information.

### Notes:
- Requires .NET 6+
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
string proxy = ...;
var httpProxy = ProxyClient.Parse(ProxyType.HTTP, proxy);
...
```

Method #3:
```csharp
using EzNetProxy.Modals;

var data = new ProxyData
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
var proxy = ProxyClient.Parse(PeoxyType.HTTP, "q.webshare.io:80");

var httpClientHandler = new HttpClientHandler
{
  Proxy = proxy,
  UseProxy = true // IMPORTANT!
};

using var client = new HttpClient(httpClientHandler, true);

var res = await client.GetAsync("https://wtfismyip.com/text");
var str = await res.Content.ReadAsStringAsync();

Console.WriteLine(str);
// str = 104.227.XXX.XXX

```

---

This project is inspired by [xNet](https://github.com/X-rus/xNet); the project is no longer maintained by its creator and even many forks of the project, such as [Leaf.xNet](https://github.com/csharp-leaf/Leaf.xNet). One of the main reasons why xNet was treasured was for its support of many proxy protocols, such as Socks-4 and Socks-5, and how simple it was to use. Hopefully, this project can be an excellent replacement for one part of the project — simple proxy usage.

Please check my profile if you'd like to contact me.
