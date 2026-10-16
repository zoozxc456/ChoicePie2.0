using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.Shared.Hosting.Extensions;

public static class ChoicePieForwardedHeadersExtensions
{
    public const string SectionName = "ForwardedHeaders";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 只信任設定檔列出的 proxy / 網段所帶的 X-Forwarded-For（預設僅 loopback），
        /// 其他來源偽造的標頭會被忽略。部署時把 Nuxt BFF 的 IP 或所在網段加進
        /// <c>ForwardedHeaders:KnownProxies</c> / <c>ForwardedHeaders:KnownNetworks</c>。
        /// </summary>
        public IServiceCollection AddChoicePieForwardedHeaders(IConfiguration configuration)
        {
            services.Configure<ForwardedHeadersOptions>(options => ChoicePieForwardedHeadersExtensions.Apply(options, configuration));
            return services;
        }
    }

    public static void Apply(ForwardedHeadersOptions options, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var proxies = section.GetSection("KnownProxies").Get<string[]>() ?? ["127.0.0.1", "::1"];
        var networks = section.GetSection("KnownNetworks").Get<string[]>() ?? [];

        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var proxy in proxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }

        foreach (var network in networks)
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }
    }
}
