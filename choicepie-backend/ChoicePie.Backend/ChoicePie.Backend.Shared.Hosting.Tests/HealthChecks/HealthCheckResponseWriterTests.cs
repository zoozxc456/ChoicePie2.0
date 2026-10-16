using System.Text.Json;
using ChoicePie.Backend.Shared.Hosting.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChoicePie.Backend.Shared.Hosting.Tests.HealthChecks;

[TestFixture]
public class HealthCheckResponseWriterTests
{
    [Test]
    public async Task WriteAsync_GivenReport_WhenCalled_ThenWritesJsonWithStatusPerCheckAndNoExceptionDetails()
    {
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            ["postgres"] = new(HealthStatus.Healthy, null, TimeSpan.FromMilliseconds(5), null, null),
            ["redis"] = new(HealthStatus.Unhealthy, "secret-host:6379 refused", TimeSpan.FromMilliseconds(7),
                new InvalidOperationException("secret-host:6379 refused"), null)
        }, TimeSpan.FromMilliseconds(12));
        var body = new MemoryStream();
        var context = new DefaultHttpContext { Response = { Body = body } };

        await HealthCheckResponseWriter.WriteAsync(context, report);

        Assert.That(context.Response.ContentType, Does.StartWith("application/json"));
        var text = System.Text.Encoding.UTF8.GetString(body.ToArray());
        Assert.That(text, Does.Not.Contain("secret-host"));
        using var doc = JsonDocument.Parse(text);
        Assert.That(doc.RootElement.GetProperty("status").GetString(), Is.EqualTo("Unhealthy"));
        var checks = doc.RootElement.GetProperty("checks");
        Assert.That(checks.GetProperty("postgres").GetProperty("status").GetString(), Is.EqualTo("Healthy"));
        Assert.That(checks.GetProperty("redis").GetProperty("status").GetString(), Is.EqualTo("Unhealthy"));
    }
}
