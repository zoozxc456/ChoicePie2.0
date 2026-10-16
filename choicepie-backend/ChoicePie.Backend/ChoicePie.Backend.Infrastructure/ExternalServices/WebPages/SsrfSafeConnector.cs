using System.Net;
using System.Net.Sockets;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

/// <summary>連線時解析出的位址落在不允許的範圍內。</summary>
public sealed class BlockedAddressException(string host, IPAddress address)
    : IOException($"Host '{host}' resolved to blocked address {address}.");

/// <summary>
/// 供 <see cref="SocketsHttpHandler.ConnectCallback"/> 使用：自行解析 DNS、驗證「每一個」解析結果，
/// 再連到已驗證過的 IP（而非讓 HttpClient 再解析一次），防止 DNS rebinding。
/// </summary>
public sealed class SsrfSafeConnector(
    Func<string, CancellationToken, Task<IPAddress[]>>? resolve = null,
    Func<IPAddress, int, CancellationToken, Task<Stream>>? connect = null)
{
    private readonly Func<string, CancellationToken, Task<IPAddress[]>> resolve =
        resolve ?? (async (host, ct) => await Dns.GetHostAddressesAsync(host, ct));

    private readonly Func<IPAddress, int, CancellationToken, Task<Stream>> connect = connect ?? ConnectSocketAsync;

    public async ValueTask<Stream> ConnectAsync(DnsEndPoint endPoint, CancellationToken cancellationToken)
    {
        var addresses = await resolve(endPoint.Host, cancellationToken);
        if (addresses.Length == 0)
        {
            throw new IOException($"Host '{endPoint.Host}' did not resolve to any address.");
        }

        foreach (var address in addresses)
        {
            if (!WebPageUrlGuard.IsAllowedAddress(address))
            {
                throw new BlockedAddressException(endPoint.Host, address);
            }
        }

        Exception? last = null;
        foreach (var address in addresses)
        {
            try
            {
                return await connect(address, endPoint.Port, cancellationToken);
            }
            catch (SocketException ex)
            {
                last = ex;
            }
        }

        throw new IOException($"Could not connect to '{endPoint.Host}'.", last);
    }

    private static async Task<Stream> ConnectSocketAsync(IPAddress address, int port, CancellationToken cancellationToken)
    {
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(new IPEndPoint(address, port), cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    /// <summary>建立不跟隨轉址、不走 Proxy、連線時驗證位址的 handler。</summary>
    public static SocketsHttpHandler CreateHandler(SsrfSafeConnector? connector = null)
    {
        connector ??= new SsrfSafeConnector();
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            ConnectTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectCallback = async (context, ct) => await connector.ConnectAsync(context.DnsEndPoint, ct)
        };
    }
}
