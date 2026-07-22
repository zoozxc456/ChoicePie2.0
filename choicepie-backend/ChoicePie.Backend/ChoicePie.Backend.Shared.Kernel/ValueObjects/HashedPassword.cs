using ChoicePie.Backend.Shared.Kernel.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.ValueObjects;

public sealed record HashedPassword : ValueObject
{
    public string Hash { get; }
    public string Salt { get; }

    private HashedPassword(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    public static HashedPassword Create(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new InvalidHashedPasswordException("雜湊值不能為空。");
        }

        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new InvalidHashedPasswordException("鹽值不能為空。");
        }

        return new HashedPassword(hash, salt);
    }
}
