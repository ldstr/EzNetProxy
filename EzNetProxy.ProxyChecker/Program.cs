using EzNetProxy;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

if (args.Length == 0)
    ExitWithMsg("No input!");

ConcurrentQueue<string> proxies = new();
ConcurrentQueue<(string Proxy, bool IsStatic, string Country)> exportQueue = new();

string input = args[0];
ProxyType? proxyType = null;
int valid = 0, invalid = 0;

CancellationTokenSource cts = new();

try
{
    if (File.Exists(input))
    {
        using var fileStream = File.OpenRead(args[0]);
        using StreamReader reader = new(fileStream);

        string? line = null;

        while (
            (line = await reader.ReadLineAsync(cts.Token)) != null
            ) if (line.Contains(':'))
                proxies.Enqueue(line);
    }
    else if (input.Contains(':')) proxies.Enqueue(input);
    else ExitWithMsg("Invalid input!");

    if (proxies.IsEmpty)
        ExitWithMsg("No proxies loaded!");

    int loadedProxies = proxies.Count;
    bool singleProxy = loadedProxies == 1;

    Console.WriteLine(
        $"Loaded {IntStr(proxies.Count)} {(singleProxy ? "proxy" : "proxies")}!"
        );

    Dictionary<ConsoleKey, ProxyType> proxyDic = new()
    {
        { ConsoleKey.D1, ProxyType.HTTP },
        { ConsoleKey.D2, ProxyType.Socks4 },
        { ConsoleKey.D3, ProxyType.Socks4A },
        { ConsoleKey.D4, ProxyType.Socks5 }
    };

    do
    {
        Console.Write("Proxy type: ");

        for (int i = 0; i < proxyDic.Count; i++)
            Console.Write(
                $"[{i + 1}] {proxyDic.Values.ElementAt(i)} "
                );

        Console.WriteLine();
        Console.Write("> ");

        var opt = Console.ReadKey(true).Key;

        if (!proxyDic.ContainsKey(opt))
        {
            Console.WriteLine("Invalid input!");
            continue;
        }

        proxyType = proxyDic[opt];
        Console.WriteLine($"Using {proxyType}...");
        break;
    }
    while (proxyType is null);

    int maxThreads = singleProxy ? 1 : 0;

    while (0 >= maxThreads)
    {
        Console.Write("Threads: ");

        if (!int.TryParse(
            Console.ReadLine(),
            out maxThreads
            )) continue;
    }

    if (maxThreads > loadedProxies)
        maxThreads = loadedProxies;

    Console.Clear();
    Console.WriteLine("Type     | Country | Proxy");
    Console.WriteLine(new string('-', 70));

    var threads = new Thread[maxThreads];

    for (int i = 0; i < threads.Length; i++)
    {
        threads[i] = new(CheckAsync().Wait);
        threads[i].Start();
    }

    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("Canceling...");
        cts.Cancel();
        //e.Cancel = false;
    };

    _ = UpdateStatusAsync();
    _ = SaveAndLogWorker();

    foreach (var i in threads)
        i.Join();

    Console.WriteLine($"Done; {DateTime.Now:G}!");
}
catch (Exception ex) { ExitWithMsg(ex.ToString()); }

async Task UpdateStatusAsync()
{
    while (true)
    {
        Console.Title = $"Valid: {IntStr(valid)}; Invalid: {IntStr(invalid)}";
        await Task.Delay(41);
    }
}

async Task SaveAndLogWorker()
{
    string expDir = Path.Combine(
        ".results",
        $"{proxyType} - {DateTime.Now:MMMM d @ h;m;s}"
        );

    Directory.CreateDirectory(expDir);
    Directory.CreateDirectory(Path.Combine(expDir, "Static"));
    Directory.CreateDirectory(Path.Combine(expDir, "Rotating"));

    async Task TrySaveAsync(string path, string content) => await File.AppendAllTextAsync(
        Path.Combine(expDir, path + ".txt"),
        content + Environment.NewLine
        );

    while (!cts.IsCancellationRequested)
    {
        await Task.Delay(1);

        if (exportQueue.IsEmpty)
            continue;

        while (!exportQueue.IsEmpty)
        {
            if (!exportQueue.TryDequeue(out var data))
                continue;

            string
                proxy = data.Proxy,
                proxyTypeStr = data.IsStatic ? "Static" : "Rotating";

            try
            {
                await TrySaveAsync(".all", proxy);

                await TrySaveAsync(
                    $"{proxyTypeStr}/.all",
                    proxy
                    );

                await TrySaveAsync(
                    $"{proxyTypeStr}/{data.Country}",
                    proxy
                    );

                Console.WriteLine(
                    "{0} | {1}      | {2}",
                    proxyTypeStr + new string(' ', 8 - proxyTypeStr.Length),
                    data.Country,
                    proxy
                    );
            }
            catch { exportQueue.Enqueue(data); }
        }
    }
}

async Task CheckAsync()
{
    while (!proxies.IsEmpty || cts.IsCancellationRequested)
    {
        if (!proxies.TryDequeue(out string? proxy))
            continue;

        try
        {
            var (isStatic, country) = await GetIpInfoAsync(proxy);
            exportQueue.Enqueue((proxy, isStatic, country));
            Interlocked.Increment(ref valid);
        }
        catch (HttpRequestException) { Interlocked.Increment(ref invalid); }
        catch { proxies.Enqueue(proxy); }
    }
}

async ValueTask<(bool IsStatic, string Country)> GetIpInfoAsync(string proxy)
{
    HttpClientHandler handler = new()
    {
        Proxy = ProxyClient.Parse(proxyType!.Value, proxy)
    };

    using HttpClient client = new(handler, true);

    var resp = await client.GetStringAsync(
        "https://wtfismyip.com/json",
        cts.Token
        );

    static string Extract(Match match) => match.Groups[1].Value;

    return (
        Extract(IpAddressRegex().Match(resp)) == proxy,
        Extract(CountryCodeRegex().Match(resp))
        );
}

static void ExitWithMsg(string message)
{
    Console.WriteLine(message);
    Exit();
}

static void Exit()
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey(true);
    Environment.Exit(0);
}

static string IntStr(int input) => input.ToString("#,##0");

partial class Program
{
    [GeneratedRegex("IPAddress\": \"(.*?)\"")]
    private static partial Regex IpAddressRegex();

    [GeneratedRegex("CountryCode\": \"(.*?)\"")]
    private static partial Regex CountryCodeRegex();
}