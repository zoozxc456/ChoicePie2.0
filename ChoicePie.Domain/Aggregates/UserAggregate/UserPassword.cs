using ChoicePie.Domain.Common;
using ChoicePie.Domain.Common.ValueObjects;

namespace ChoicePie.Domain.Aggregates.UserAggregate;

public class UserPassword : Entity
{
    public Guid UserId { get; private set; }
    public string HashedPassword { get; private set; }
    public string Salted { get; set; }

    private UserPassword()
    {
    }

    public UserPassword(Guid userId, string hashedPassword, string salted)
    {
        UserId = userId;
        HashedPassword = hashedPassword;
        Salted = salted;
        AuditInfo = new AuditInfo(DateTime.UtcNow, Id, DateTime.Now, Id);
    }

    public void ChangePassword(string hashedPassword, string salted)
    {
        if (string.IsNullOrEmpty(hashedPassword)) throw new ArgumentException(nameof(hashedPassword));
        if (string.IsNullOrEmpty(salted)) throw new ArgumentException(nameof(salted));

        HashedPassword = hashedPassword;
        Salted = salted;

        AuditInfo = AuditInfo.Touch();
    }
}