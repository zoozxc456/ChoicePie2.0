namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IPasswordHasher
{
    (string hash, string salt) Hash(string password);
    bool Verify(string password, string passwordHash,string salt);
}
