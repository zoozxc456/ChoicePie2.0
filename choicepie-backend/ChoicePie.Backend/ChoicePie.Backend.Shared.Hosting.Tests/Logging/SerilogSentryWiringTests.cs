using System.Collections.Concurrent;
using System.Net;
using ChoicePie.Backend.Shared.Hosting.Logging;
using ChoicePie.Backend.Shared.Hosting.Monitoring;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;
using Sentry.Protocol.Envelopes;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Logging;

/// <summary>
/// Serilog 取代 MEL providers 後，Sentry 原本的 logging 整合會默默失效；這裡用真的 host 確認
/// LogError 仍會變成 Sentry 事件，且 BeforeSend 過濾照樣生效。
/// </summary>
[TestFixture]
[NonParallelizable] // SentrySdk 是 process 內的全域狀態。
public class SerilogSentryWiringTests
{
    private sealed class FakeDomainException()
        : DomainException("internal", "friendly", "FAKE", HttpStatusCode.BadRequest);

    private sealed class RecordingTransport : ITransport
    {
        public ConcurrentBag<Envelope> Envelopes { get; } = [];

        public Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken = default)
        {
            Envelopes.Add(envelope);
            return Task.CompletedTask;
        }

        public int EventCount => Envelopes.SelectMany(e => e.Items).Count(i => i.TryGetType() == "event");
    }

    private RecordingTransport _transport = null!;
    private WebApplication _app = null!;

    [SetUp]
    public async Task SetUp()
    {
        _transport = new RecordingTransport();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Configuration["Sentry:Dsn"] = "https://key@o0.ingest.sentry.io/0";
        builder.Configuration["LogOutput:Format"] = "Text";
        builder.AddChoicePieSentry();
        builder.AddChoicePieSerilog();
        builder.Services.PostConfigure<SentryAspNetCoreOptions>(options => options.Transport = _transport);

        _app = builder.Build();
        await _app.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Test]
    public async Task LogError_GivenUnexpectedException_WhenLogged_ThenSentToSentry()
    {
        Logger().LogError(new NullReferenceException("boom"), "Unexpected failure");

        await FlushAsync();

        Assert.That(_transport.EventCount, Is.EqualTo(1));
    }

    [Test]
    public async Task LogError_GivenDomainException_WhenLogged_ThenFilteredOut()
    {
        Logger().LogError(new FakeDomainException(), "Expected failure");

        await FlushAsync();

        Assert.That(_transport.EventCount, Is.Zero);
    }

    [Test]
    public async Task LogWarning_GivenAnyMessage_WhenLogged_ThenNotSentAsEvent()
    {
        Logger().LogWarning(new NullReferenceException("boom"), "Just a warning");

        await FlushAsync();

        Assert.That(_transport.EventCount, Is.Zero);
    }

    private ILogger Logger() => _app.Services.GetRequiredService<ILogger<SerilogSentryWiringTests>>();

    private Task FlushAsync() => _app.Services.GetRequiredService<IHub>().FlushAsync(TimeSpan.FromSeconds(5));
}
