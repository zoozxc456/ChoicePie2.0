using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

public sealed class SendEmailVerificationEmailHandler(IEmailSender emailSender, IFrontendUrlProvider frontendUrlProvider)
    : INotificationHandler<DomainEventNotification<EmailVerificationRequestedDomainEvent>>
{
    public Task Handle(DomainEventNotification<EmailVerificationRequestedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var verifyLink = $"{frontendUrlProvider.BaseUrl}/verify-email?token={Uri.EscapeDataString(domainEvent.RawToken)}";

        var htmlBody = $"""
            <p>歡迎加入 ChoicePie！</p>
            <p>請點擊以下連結完成 Email 驗證（24 小時內有效）：</p>
            <p><a href="{verifyLink}">{verifyLink}</a></p>
            <p>若您並未註冊 ChoicePie 帳號，請忽略此信件。</p>
            """;

        return emailSender.SendAsync(domainEvent.Email, "驗證您的 ChoicePie 帳號", htmlBody, cancellationToken);
    }
}
