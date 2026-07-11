namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IRefreshTokenGenerator
{
    (string RawToken, string TokenHash) Generate();
    string Hash(string rawToken);
}
