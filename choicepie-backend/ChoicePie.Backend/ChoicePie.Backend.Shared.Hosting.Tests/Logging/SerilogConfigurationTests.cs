using System.Diagnostics;
using ChoicePie.Backend.Shared.Hosting.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Logging;

[TestFixture]
public class SerilogConfigurationTests
{
    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    [TestCase("Development", LogOutputFormat.Text)]
    [TestCase("Production", LogOutputFormat.Json)]
    [TestCase("Staging", LogOutputFormat.Json)]
    public void ResolveFormat_GivenNoOverride_WhenResolved_ThenDependsOnEnvironment(string environmentName, LogOutputFormat expected)
        => Assert.That(SerilogConfiguration.ResolveFormat(Configuration(), environmentName), Is.EqualTo(expected));

    [Test]
    public void ResolveFormat_GivenOverride_WhenResolved_ThenOverrideWins()
    {
        var configuration = Configuration(("LogOutput:Format", "Json"));

        Assert.That(SerilogConfiguration.ResolveFormat(configuration, "Development"), Is.EqualTo(LogOutputFormat.Json));
    }

    [Test]
    public void Configure_GivenActivity_WhenLogged_ThenEventCarriesEnrichmentAndTraceId()
    {
        var sink = new CollectingSink();
        using var logger = CreateLogger(Configuration(), sink);
        using var activity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();

        logger.Information("hello");

        var logEvent = sink.Events.Single();
        Assert.That(logEvent.Properties["Application"].ToString(), Is.EqualTo("\"ChoicePie.Backend.WebApi\""));
        Assert.That(logEvent.Properties["Environment"].ToString(), Is.EqualTo("\"Production\""));
        Assert.That(logEvent.Properties["MachineName"].ToString(), Is.EqualTo($"\"{Environment.MachineName}\""));
        Assert.That(logEvent.Properties, Does.ContainKey("Version"));
        Assert.That(logEvent.TraceId, Is.EqualTo(activity.TraceId));
    }

    [Test]
    public void Configure_GivenSerilogSectionOverride_WhenFrameworkLogsBelowIt_ThenDropped()
    {
        var sink = new CollectingSink();
        var configuration = Configuration(
            ("Serilog:MinimumLevel:Default", "Information"),
            ("Serilog:MinimumLevel:Override:Microsoft.AspNetCore", "Warning"));
        using var logger = CreateLogger(configuration, sink);

        logger.ForContext(Constants.SourceContextPropertyName, "Microsoft.AspNetCore.Routing").Information("noise");
        logger.ForContext(Constants.SourceContextPropertyName, "ChoicePie.Backend.App").Information("kept");

        Assert.That(sink.Events.Select(e => e.MessageTemplate.Text), Is.EqualTo(new[] { "kept" }));
    }

    private static Logger CreateLogger(IConfiguration configuration, ILogEventSink sink)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.ApplicationName.Returns("ChoicePie.Backend.WebApi");
        environment.EnvironmentName.Returns("Production");

        return SerilogConfiguration.Configure(new LoggerConfiguration(), configuration, environment)
            .WriteTo.Sink(sink)
            .CreateLogger();
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();
}
