using System.Text.Json;
using ChoicePie.Backend.Shared.Hosting.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Logging;

/// <summary>
/// 端到端確認 Loki 依賴的輸出契約：正式環境的 stdout 每個 request 一行 JSON，帶 route pattern 與狀態碼。
/// </summary>
[TestFixture]
[NonParallelizable] // 會替換全域的 Console.Out。
public class RequestLoggingPipelineTests
{
    private TextWriter _originalOut = null!;
    private StringWriter _stdout = null!;

    [SetUp]
    public void SetUp()
    {
        _originalOut = Console.Out;
        _stdout = new StringWriter();
        Console.SetOut(_stdout);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalOut);
        _stdout.Dispose();
    }

    [Test]
    public async Task Request_GivenProductionHost_WhenCompleted_ThenWritesOneJsonSummaryLine()
    {
        await using var app = await StartAppAsync();

        await app.GetTestClient().GetAsync("/api/v1/quizzes/42");
        await app.GetTestClient().GetAsync("/health/live");
        // 摘要在 response 送出之後才寫，等 host 收完所有進行中的 request 再讀。
        await app.StopAsync();

        var requestLines = JsonLines().Where(line =>
            line.TryGetProperty("SourceContext", out var source)
            && source.GetString() == "Serilog.AspNetCore.RequestLoggingMiddleware").ToList();

        Assert.That(requestLines, Has.Count.EqualTo(1), "health 探針不應被記錄");
        var line = requestLines[0];
        Assert.That(line.GetProperty("RoutePattern").GetString(), Is.EqualTo("/api/v1/quizzes/{id}"));
        Assert.That(line.GetProperty("StatusCode").GetInt32(), Is.EqualTo(200));
        Assert.That(line.GetProperty("Environment").GetString(), Is.EqualTo("Production"));
        Assert.That(line.GetProperty("@l").GetString(), Is.EqualTo("Information"));
        Assert.That(line.GetProperty("@m").GetString(), Does.StartWith("HTTP GET /api/v1/quizzes/42 responded 200"));
        Assert.That(line.TryGetProperty("@tr", out _), Is.True);
    }

    private static async Task<WebApplication> StartAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.AddChoicePieSerilog();

        var app = builder.Build();
        app.UseChoicePieRequestLogging();
        app.MapGet("/api/v1/quizzes/{id}", (int id) => Results.Ok(id));
        app.MapGet("/health/live", () => Results.Ok());

        await app.StartAsync();
        return app;
    }

    private List<JsonElement> JsonLines()
        => _stdout.ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.StartsWith('{'))
            .Select(line => JsonDocument.Parse(line).RootElement)
            .ToList();
}
