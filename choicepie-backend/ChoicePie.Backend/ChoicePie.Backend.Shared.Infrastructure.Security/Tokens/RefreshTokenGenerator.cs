using System.Security.Cryptography;
using System.Text;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Shared.Infrastructure.Security.Tokens;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator, ISingletonDependency
{
    private const int RawTokenByteLength = 32;

    public (string RawToken, string TokenHash) Generate()
    {
        var rawToken = GenerateRawToken();
        return (rawToken, Hash(rawToken));
    }

    public string Hash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[RawTokenByteLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
