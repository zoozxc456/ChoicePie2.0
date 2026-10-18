using System.Net.Http.Json;

namespace ChoicePie.Backend.WebApi.Tests;

/// <summary>
/// 正式的 outbox 接線走一整圈：事件跟業務資料同交易寫入 → payload 用 Redis 上共用的 Data Protection 金鑰加密
/// → 背景 processor 認領、解密、派送給可靠 handler → 信寄出、outbox 清空。
/// </summary>
public sealed class OutboxIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _factory.DisposeAsync();

    [Test]
    public async Task Register_GivenNewMember_WhenOutboxDrains_ThenVerificationEmailIsSentWithTokenLink()
    {
        using var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        await RegisterAsync(client, email);
        await _factory.WaitForOutboxDrainedAsync();

        Assert.That(_factory.Emails.Sent.Where(m => m.To == email).Select(m => m.HtmlBody),
            Has.One.Contains("/verify-email?token="));
    }

    [Test]
    public async Task ForgotPassword_GivenRegisteredMember_WhenOutboxDrains_ThenResetEmailIsSentWithTokenLink()
    {
        using var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await RegisterAsync(client, email);

        (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { Email = email })).EnsureSuccessStatusCode();
        await _factory.WaitForOutboxDrainedAsync();

        Assert.That(_factory.Emails.Sent.Where(m => m.To == email).Select(m => m.HtmlBody),
            Has.One.Contains("/reset-password?token="));
    }

    private static async Task RegisterAsync(HttpClient client, string email) =>
        (await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Outbox Member",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        })).EnsureSuccessStatusCode();
}
