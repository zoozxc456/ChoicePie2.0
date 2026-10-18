using ChoicePie.Backend.Application.Identity.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Events;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

// 寄信改由 Outbox 可靠派送：SMTP 暫時掛掉不會讓請求 500，也不會讓信就此消失。
[TestFixture]
public class SendAccountEmailHandlersTests
{
    private IEmailSender _emailSender = null!;
    private IFrontendUrlProvider _frontendUrlProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _frontendUrlProvider = Substitute.For<IFrontendUrlProvider>();
        _frontendUrlProvider.BaseUrl.Returns("https://choicepie.test");
    }

    [Test]
    public async Task SendPasswordResetEmail_GivenResetRequested_WhenHandled_ThenSendsLinkWithEscapedRawToken()
    {
        var sut = new SendPasswordResetEmailHandler(_emailSender, _frontendUrlProvider);

        await sut.Handle(new ReliableDomainEventNotification<PasswordResetRequestedDomainEvent>(
            new PasswordResetRequestedDomainEvent(Guid.NewGuid(), "member@example.com", "a+b/c")), CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            "member@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("https://choicepie.test/reset-password?token=a%2Bb%2Fc")),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SendEmailVerificationEmail_GivenVerificationRequested_WhenHandled_ThenSendsLinkWithEscapedRawToken()
    {
        var sut = new SendEmailVerificationEmailHandler(_emailSender, _frontendUrlProvider);

        await sut.Handle(new ReliableDomainEventNotification<EmailVerificationRequestedDomainEvent>(
            new EmailVerificationRequestedDomainEvent(Guid.NewGuid(), "member@example.com", "a+b/c")), CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            "member@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("https://choicepie.test/verify-email?token=a%2Bb%2Fc")),
            Arg.Any<CancellationToken>());
    }
}
