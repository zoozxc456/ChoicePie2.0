using System.Net;
using System.Net.Sockets;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

public enum WebPageUrlCheck
{
    Allowed,

    /// <summary>不是絕對的 http/https 網址。</summary>
    Unsupported,

    /// <summary>帶有帳密、不在允許清單的連接埠（僅 80 / 443 / 8080 / 8443）、或主機為內網 / 本機位址等不允許存取的目標。</summary>
    Denied
}

/// <summary>
/// SSRF 防護的純邏輯：判斷網址與 IP 位址是否允許連線。無任何 I/O，方便單元測試。
/// </summary>
public static class WebPageUrlGuard
{
    public static WebPageUrlCheck CheckUrl(Uri? url)
    {
        if (url is null || !url.IsAbsoluteUri) return WebPageUrlCheck.Unsupported;
        if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps) return WebPageUrlCheck.Unsupported;
        if (string.IsNullOrWhiteSpace(url.Host)) return WebPageUrlCheck.Unsupported;

        if (!string.IsNullOrEmpty(url.UserInfo)) return WebPageUrlCheck.Denied;
        if (!IsAllowedPort(url)) return WebPageUrlCheck.Denied;

        var host = url.Host.TrimEnd('.');
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            return WebPageUrlCheck.Denied;
        }

        if (IPAddress.TryParse(url.Host.Trim('[', ']'), out var literal) && !IsAllowedAddress(literal))
        {
            return WebPageUrlCheck.Denied;
        }

        return WebPageUrlCheck.Allowed;
    }

    /// <summary>預設連接埠（80 / 443）以及常見的 8080 / 8443；真正的 SSRF 防線仍是 IP 位址檢查。</summary>
    private static bool IsAllowedPort(Uri url) => url.IsDefaultPort || url.Port is 8080 or 8443;

    public static bool IsAllowedAddress(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return IsAllowedIPv4(address.GetAddressBytes());
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6) return false;

        if (address.IsIPv4MappedToIPv6) return IsAllowedIPv4(address.MapToIPv4().GetAddressBytes());

        if (IPAddress.IPv6Loopback.Equals(address) || IPAddress.IPv6None.Equals(address)
            || address.IsIPv6Multicast || address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
        {
            return false;
        }

        var bytes = address.GetAddressBytes();

        // ULA fc00::/7
        if ((bytes[0] & 0xFE) == 0xFC) return false;

        // 內嵌 IPv4 的過渡位址：IPv4-compatible (::/96，含 ::、::1)、NAT64 (64:ff9b::/96)、6to4 (2002::/16)
        if (IsAllZero(bytes, 0, 12)) return IsAllowedIPv4(bytes[12..16]);

        if (bytes[0] == 0x00 && bytes[1] == 0x64 && bytes[2] == 0xFF && bytes[3] == 0x9B && IsAllZero(bytes, 4, 8))
        {
            return IsAllowedIPv4(bytes[12..16]);
        }

        if (bytes[0] == 0x20 && bytes[1] == 0x02) return IsAllowedIPv4(bytes[2..6]);

        return true;
    }

    private static bool IsAllowedIPv4(byte[] b)
    {
        return b[0] switch
        {
            0 => false, // 0.0.0.0/8
            10 => false, // 10.0.0.0/8
            127 => false, // loopback
            >= 224 => false, // multicast、保留區與 255.255.255.255 廣播
            100 when (b[1] & 0xC0) == 64 => false, // CGNAT 100.64.0.0/10
            169 when b[1] == 254 => false, // link-local / 雲端 metadata
            172 when (b[1] & 0xF0) == 16 => false, // 172.16.0.0/12
            192 when b[1] == 168 => false, // 192.168.0.0/16
            192 when b[1] == 0 && b[2] == 0 => false, // 192.0.0.0/24
            198 when (b[1] & 0xFE) == 18 => false, // 198.18.0.0/15 benchmarking
            _ => true
        };
    }

    private static bool IsAllZero(byte[] bytes, int start, int length)
    {
        for (var i = start; i < start + length; i++)
        {
            if (bytes[i] != 0) return false;
        }

        return true;
    }
}
