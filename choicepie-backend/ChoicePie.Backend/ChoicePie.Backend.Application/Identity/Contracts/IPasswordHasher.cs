namespace ChoicePie.Backend.Application.Identity.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
