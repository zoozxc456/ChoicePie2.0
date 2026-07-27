using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Events;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.EventHandlers;

public sealed class SendPasswordResetEmailHandler(IEmailSender emailSender, IFrontendUrlProvider frontendUrlProvider)
    : INotificationHandler<DomainEventNotification<PasswordResetRequestedDomainEvent>>
{
    public Task Handle(DomainEventNotification<PasswordResetRequestedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var resetLink = $"{frontendUrlProvider.BaseUrl}/reset-password?token={Uri.EscapeDataString(domainEvent.RawToken)}";

        var htmlBody = $"""
            <p>您好，</p>
            <p>我們收到您重設 ChoicePie 帳號密碼的請求，請點擊以下連結重設密碼（30 分鐘內有效）：</p>
            <p><a href="{resetLink}">{resetLink}</a></p>
            <p>若您並未提出此請求，請忽略此信件。</p>
            """;

        return emailSender.SendAsync(domainEvent.Email, "重設您的 ChoicePie 密碼", htmlBody, cancellationToken);
    }
}
