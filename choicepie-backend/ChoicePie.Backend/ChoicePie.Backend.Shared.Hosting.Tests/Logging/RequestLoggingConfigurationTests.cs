using System.Net;
using System.Security.Claims;
using ChoicePie.Backend.Shared.Hosting.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using NSubstitute;
using Serilog;
using Serilog.Events;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Logging;

[TestFixture]
public class RequestLoggingConfigurationTests
{
    [TestCase("/health/live")]
    [TestCase("/health/ready")]
    public void GetLevel_GivenHealthProbe_WhenCompleted_ThenVerbose(string path)
    {
        var httpContext = new DefaultHttpContext { Request = { Path = path } };

        Assert.That(RequestLoggingConfiguration.GetLevel(httpContext, 1, null), Is.EqualTo(LogEventLevel.Verbose));
    }

    [TestCase(200)]
    [TestCase(404)]
    [TestCase(429)]
    public void GetLevel_GivenNonServerErrorStatus_WhenCompleted_ThenInformation(int statusCode)
    {
        var httpContext = new DefaultHttpContext { Request = { Path = "/api/v1/quizzes" }, Response = { StatusCode = statusCode } };

        Assert.That(RequestLoggingConfiguration.GetLevel(httpContext, 1, null), Is.EqualTo(LogEventLevel.Information));
    }

    [Test]
    public void GetLevel_GivenServerError_WhenCompleted_ThenWarningSoSentryIsNotReportedTwice()
    {
        var httpContext = new DefaultHttpContext { Request = { Path = "/api/v1/quizzes" }, Response = { StatusCode = 500 } };

        Assert.That(RequestLoggingConfiguration.GetLevel(httpContext, 1, null), Is.EqualTo(LogEventLevel.Warning));
    }

    [Test]
    public void GetLevel_GivenEscapedException_WhenCompleted_ThenWarning()
    {
        var httpContext = new DefaultHttpContext { Request = { Path = "/api/v1/quizzes" } };

        Assert.That(RequestLoggingConfiguration.GetLevel(httpContext, 1, new NullReferenceException()),
            Is.EqualTo(LogEventLevel.Warning));
    }

    [Test]
    public void EnrichDiagnosticContext_GivenAuthenticatedRoutedRequest_WhenEnriched_ThenSetsIdsAndRoutePattern()
    {
        var diagnosticContext = Substitute.For<IDiagnosticContext>();
        var httpContext = new DefaultHttpContext
        {
            Request = { Host = new HostString("api.choicepie.test") },
            Connection = { RemoteIpAddress = IPAddress.Parse("203.0.113.7") },
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "member-1"), new Claim("email", "a@b.c")], "test"))
        };
        httpContext.SetEndpoint(new RouteEndpoint(
            _ => Task.CompletedTask, RoutePatternFactory.Parse("api/v{version}/quizzes/{id}"), 0, null, null));

        RequestLoggingConfiguration.EnrichDiagnosticContext(diagnosticContext, httpContext);

        diagnosticContext.Received().Set("RequestHost", "api.choicepie.test");
        diagnosticContext.Received().Set("ClientIp", "203.0.113.7");
        diagnosticContext.Received().Set("RoutePattern", "api/v{version}/quizzes/{id}");
        diagnosticContext.Received().Set("UserId", "member-1");
        diagnosticContext.DidNotReceive().Set(Arg.Any<string>(), "a@b.c", Arg.Any<bool>());
    }

    [Test]
    public void EnrichDiagnosticContext_GivenAnonymousRequest_WhenEnriched_ThenDoesNotSetUserId()
    {
        var diagnosticContext = Substitute.For<IDiagnosticContext>();
        var httpContext = new DefaultHttpContext();

        RequestLoggingConfiguration.EnrichDiagnosticContext(diagnosticContext, httpContext);

        diagnosticContext.DidNotReceive().Set("UserId", Arg.Any<object?>(), Arg.Any<bool>());
        diagnosticContext.DidNotReceive().Set("RoutePattern", Arg.Any<object?>(), Arg.Any<bool>());
    }
}
