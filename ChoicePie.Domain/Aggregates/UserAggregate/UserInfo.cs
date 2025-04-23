using ChoicePie.Domain.Common;
using ChoicePie.Domain.Common.ValueObjects;

namespace ChoicePie.Domain.Aggregates.UserAggregate;

public class UserInfo : Entity
{
    public Guid UserId { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string? Favorite { get; private set; }

    private UserInfo()
    {
    }

    public UserInfo(Guid userId, string username, string email, string? favorite)
    {
        UserId = userId;
        Username = username;
        Email = email;
        Favorite = favorite;
        AuditInfo = new AuditInfo(DateTime.UtcNow, Id, DateTime.Now, Id);
    }

    public void ChangeUsername(string username)
    {
        if (string.IsNullOrEmpty(username)) throw new ArgumentException(nameof(username));

        Username = username;
        AuditInfo = AuditInfo.Touch();
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentException(nameof(email));

        Email = email;
        AuditInfo = AuditInfo.Touch();
    }

    public void ChangeFavorite(string favorite)
    {
        if (string.IsNullOrEmpty(favorite)) throw new ArgumentException(nameof(favorite));

        Favorite = favorite;
        AuditInfo = AuditInfo.Touch();
    }
}