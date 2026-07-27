using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Auth;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class AdminAuthControllerTests
{
    private const string SeededPassword = "AdminPassword123!";

    private CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _factory.DisposeAsync();

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

    /// <summary>
    /// AdminAuthController 沒有 register 端點（CreateAdminUserCommandHandler 要求呼叫者已經是登入中的
    /// admin，用來新增「非第一個」admin；第一個帳號改由 AdminUserSeeder 在啟動時建立），所以測試帳號
    /// 直接透過 DbContext 種資料，繞過 HTTP。
    /// </summary>
    private async Task<string> SeedAdminAsync(string email, string password = SeededPassword)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var adminUser = AdminUser.Create("Test Admin", AdminRole.Admin);
        var hashedPassword = passwordHasher.Hash(password);
        var adminAuthAccount = AdminAuthAccount.Create(Email.Create(email), hashedPassword, adminUser.Id);

        dbContext.Add(adminUser);
        dbContext.Add(adminAuthAccount);
        await dbContext.SaveChangesAsync();

        return email;
    }

    [Test]
    public async Task LoginAsync_GivenValidCredentials_WhenCalled_ThenSetsHttpOnlyCookiesAndReturnsAdminUser()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedAdminAsync(email);
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/auth/login", new { Email = email, Password = SeededPassword });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
        Assert.That(
            setCookieHeaders.Any(c =>
                c.StartsWith($"{AuthCookieNames.AccessToken}=") && c.Contains("httponly", StringComparison.OrdinalIgnoreCase)),
            Is.True);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminLoginResultDto>>();
        Assert.That(result!.Data!.AdminUser.Email, Is.EqualTo(email));
        Assert.That(result.Data.AdminUser.Role, Is.EqualTo("admin"));
        Assert.That(result.Data.AccessToken, Is.Not.Empty);
        Assert.That(result.Data.RefreshToken, Is.Not.Empty);
    }

    [Test]
    public async Task LoginAsync_GivenWrongPassword_WhenCalled_ThenReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedAdminAsync(email);
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/auth/login", new { Email = email, Password = "WrongPassword123!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("ADMIN_INVALID_CREDENTIALS"));
    }

    [Test]
    public async Task LoginAsync_GivenUnknownEmail_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/auth/login",
            new { Email = $"{Guid.NewGuid()}@example.com", Password = SeededPassword });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("ADMIN_INVALID_CREDENTIALS"));
    }

    private static HttpRequestMessage AdminRefreshRequest(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/auth/refresh");
        request.Headers.Add(AuthHeaderNames.RefreshToken, refreshToken);
        return request;
    }

    private static async Task<string> LoginAdminAsync(HttpClient client, string email, string password = SeededPassword)
    {
        var response = await client.PostAsJsonAsync("/api/v1/admin/auth/login", new { Email = email, Password = password });
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminLoginResultDto>>();
        return result!.Data!.RefreshToken;
    }

    [Test]
    public async Task RefreshAsync_GivenValidRefreshTokenHeaderFromPriorLogin_WhenCalled_ThenRotatesCookies()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedAdminAsync(email);
        using var client = CreateClient();
        var refreshToken = await LoginAdminAsync(client, email);

        var response = await client.SendAsync(AdminRefreshRequest(refreshToken));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
        Assert.That(setCookieHeaders.Any(c => c.StartsWith($"{AuthCookieNames.AccessToken}=")), Is.True);
        Assert.That(setCookieHeaders.Any(c => c.StartsWith($"{AuthCookieNames.RefreshToken}=")), Is.True);
    }

    [Test]
    public async Task RefreshAsync_GivenNoRefreshTokenHeader_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/api/v1/admin/auth/refresh", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task RefreshAsync_GivenMemberOwnedRefreshToken_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Test Member",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<ChoicePie.Backend.Application.Identity.Dtos.LoginResultDto>>();

        var response = await client.SendAsync(AdminRefreshRequest(loginResult!.Data!.RefreshToken));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("INVALID_REFRESH_TOKEN"));
    }

    [Test]
    public async Task LogoutAsync_GivenLoggedInSession_WhenCalledThenRefreshIsRetried_ThenRefreshFails()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedAdminAsync(email);
        using var client = CreateClient();
        var refreshToken = await LoginAdminAsync(client, email);

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/auth/logout");
        logoutRequest.Headers.Add(AuthHeaderNames.RefreshToken, refreshToken);
        var logoutResponse = await client.SendAsync(logoutRequest);
        Assert.That(logoutResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var refreshAfterLogout = await client.SendAsync(AdminRefreshRequest(refreshToken));
        Assert.That(refreshAfterLogout.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetCurrentAdminUserAsync_GivenLoggedInSession_WhenCalled_ThenReturnsCurrentAdminUser()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await SeedAdminAsync(email);
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/v1/admin/auth/login", new { Email = email, Password = SeededPassword });

        var response = await client.GetAsync("/api/v1/admin/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminUserDto>>();
        Assert.That(result!.Data!.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task GetCurrentAdminUserAsync_GivenNoSession_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/v1/admin/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetCurrentAdminUserAsync_GivenMemberOwnedSession_WhenCalled_ThenReturnsForbidden()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Test Member",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });
        await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });

        var response = await client.GetAsync("/api/v1/admin/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
