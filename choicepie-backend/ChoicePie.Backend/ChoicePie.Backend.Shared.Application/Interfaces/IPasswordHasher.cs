using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IPasswordHasher
{
    HashedPassword Hash(string password);
    bool Verify(string password, HashedPassword hashedPassword);
}
