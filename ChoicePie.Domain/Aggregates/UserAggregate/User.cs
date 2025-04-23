using ChoicePie.Domain.Common;
using ChoicePie.Domain.Common.ValueObjects;

namespace ChoicePie.Domain.Aggregates.UserAggregate;

public class User : AggregateRoot
{
    public string Account { get; private set; }
    public UserInfo Info { get; private set; }
    public UserPassword Password { get; private set; }

    private User()
    {
    }

    private User(string account, UserInfo info, UserPassword password)
    {
        Account = account;
        AuditInfo = new AuditInfo(DateTime.UtcNow, Id, DateTime.Now, Id);
    }

    public static User Create(string account, UserInfo info, UserPassword password)
    {
        return new User(account, info, password);
    }
}