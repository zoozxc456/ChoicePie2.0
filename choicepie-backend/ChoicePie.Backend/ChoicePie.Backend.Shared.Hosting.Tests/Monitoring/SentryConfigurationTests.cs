using System.Net;
using ChoicePie.Backend.Shared.Application.Exceptions;
using ChoicePie.Backend.Shared.Hosting.Monitoring;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Sentry;
using Sentry.AspNetCore;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Monitoring;

[TestFixture]
public class SentryConfigurationTests
{
    private sealed class FakeDomainException()
        : DomainException("internal", "friendly", "FAKE", HttpStatusCode.BadRequest);

    [Test]
    public void Configure_GivenSettings_WhenApplied_ThenDisablesPiiAndUsesGivenValues()
    {
        var options = new SentryAspNetCoreOptions { SendDefaultPii = true };
        var settings = new SentrySettings { Dsn = "https://key@example.ingest.sentry.io/1", TracesSampleRate = 0.25 };

        SentryConfiguration.Configure(options, settings, "Production");

        Assert.That(options.Dsn, Is.EqualTo(settings.Dsn));
        Assert.That(options.SendDefaultPii, Is.False);
        Assert.That(options.Environment, Is.EqualTo("Production"));
        Assert.That(options.TracesSampleRate, Is.EqualTo(0.25));
    }

    [Test]
    public void Defaults_GivenNewSettings_WhenRead_ThenDsnIsEmptyAndTracingOff()
    {
        var settings = new SentrySettings();

        Assert.That(settings.Dsn, Is.Empty);
        Assert.That(settings.TracesSampleRate, Is.EqualTo(0));
    }

    [Test]
    public void ShouldReport_GivenDomainException_WhenChecked_ThenReturnsFalse()
        => Assert.That(SentryConfiguration.ShouldReport(new FakeDomainException()), Is.False);

    [Test]
    public void ShouldReport_GivenValidationAndBadRequestExceptions_WhenChecked_ThenReturnsFalse()
    {
        Assert.That(SentryConfiguration.ShouldReport(
            new System.ComponentModel.DataAnnotations.ValidationException("x")), Is.False);
        Assert.That(SentryConfiguration.ShouldReport(new InvalidOperationException("x")), Is.False);
        Assert.That(SentryConfiguration.ShouldReport(new OperationCanceledException()), Is.False);
    }

    [Test]
    public void ShouldReport_GivenWrappedDomainException_WhenChecked_ThenReturnsFalse()
        => Assert.That(SentryConfiguration.ShouldReport(new AggregateException(new FakeDomainException())), Is.False);

    [Test]
    public void ShouldReport_GivenUnexpectedException_WhenChecked_ThenReturnsTrue()
    {
        Assert.That(SentryConfiguration.ShouldReport(new NullReferenceException()), Is.True);
        Assert.That(SentryConfiguration.ShouldReport(null), Is.True);
    }

    [Test]
    public void BeforeSend_GivenEventWithDomainException_WhenFiltered_ThenReturnsNull()
    {
        var sentryEvent = new SentryEvent(new FakeDomainException());

        Assert.That(SentryConfiguration.BeforeSend(sentryEvent, new SentryHint()), Is.Null);
    }

    [Test]
    public void BeforeSend_GivenEventWithUnexpectedException_WhenFiltered_ThenKeepsEvent()
    {
        var sentryEvent = new SentryEvent(new NullReferenceException());

        Assert.That(SentryConfiguration.BeforeSend(sentryEvent, new SentryHint()), Is.SameAs(sentryEvent));
    }
}
