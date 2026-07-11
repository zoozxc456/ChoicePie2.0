using System.Security.Cryptography;
using System.Text;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Konscious.Security.Cryptography;

namespace ChoicePie.Backend.Shared.Infrastructure.Security.Hashers;

public sealed class Argo2IdPasswordHasher : IPasswordHasher, ISingletonDependency
{
    private const int DegreeOfParallelism = 2;
    private const int MemorySize = 64 * 1024;
    private const int Iterations = 4;
    private const int HashByteLength = 32;
    private const int DefaultSaltLength = 16;

    public (string hash, string salt) Hash(string password)
    {
        var salt = GenerateSalt();
        var hash = HashInternal(password, salt);
        return (hash, salt);
    }

    public bool Verify(string password, string passwordHash, string salt)
    {
        var computed = HashInternal(password, salt);
        var computedBytes = Encoding.UTF8.GetBytes(computed);
        var storedBytes = Encoding.UTF8.GetBytes(passwordHash);

        if (computedBytes.Length != storedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(computedBytes, storedBytes);
    }

    private string HashInternal(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Encoding.UTF8.GetBytes(salt);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var hashBytes = argon2.GetBytes(HashByteLength);
        return Convert.ToBase64String(hashBytes);
    }

    private string GenerateSalt(int length = DefaultSaltLength)
    {
        var salt = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }
}