using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChoicePie.Backend.WebApi.Tests;

public sealed class AuthEndpointsTests
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

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

    private static async Task<HttpClient> RegisterAndLoginAsync(HttpClient client, string email)
    {
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Test Member",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        return client;
    }

    [Test]
    public async Task RegisterAsync_GivenNewEmail_WhenCalled_ThenReturnsCreatedMember()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Test Member",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MemberDto>>();
        Assert.That(result!.Data!.Email, Is.EqualTo(email));
        Assert.That(result.Data.Name, Is.EqualTo("Test Member"));
    }

    [Test]
    public async Task RegisterAsync_GivenAlreadyRegisteredEmail_WhenCalled_ThenReturnsConflict()
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

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Another Name",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.That(body!.Code, Is.EqualTo("EMAIL_ALREADY_REGISTERED"));
    }

    [Test]
    public async Task RegisterAsync_GivenMismatchedPasswordConfirmation_WhenCalled_ThenReturnsBadRequest()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = $"{Guid.NewGuid()}@example.com",
            Name = "Test Member",
            Password = "Password123!",
            ConfirmPassword = "SomethingElse123!"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task LoginAsync_GivenValidCredentials_WhenCalled_ThenSetsHttpOnlyCookiesAndOmitsTokensFromBody()
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

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = "Password123!" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
        Assert.That(
            setCookieHeaders.Any(c =>
                c.StartsWith($"{AuthCookieNames.AccessToken}=") && c.Contains("httponly", StringComparison.OrdinalIgnoreCase)),
            Is.True, "access_token cookie 應該是 HttpOnly");
        Assert.That(
            setCookieHeaders.Any(c =>
                c.StartsWith($"{AuthCookieNames.RefreshToken}=") && c.Contains("httponly", StringComparison.OrdinalIgnoreCase)),
            Is.True, "refresh_token cookie 應該是 HttpOnly");

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("accessToken"));
        Assert.That(body, Does.Not.Contain("refreshToken"));

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MemberDto>>();
        Assert.That(result!.Data!.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task RefreshAsync_GivenValidCookieFromPriorLogin_WhenCalled_ThenRotatesCookies()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await RegisterAndLoginAsync(client, email);

        var response = await client.PostAsync("/api/v1/auth/refresh", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
        Assert.That(setCookieHeaders.Any(c => c.StartsWith($"{AuthCookieNames.AccessToken}=")), Is.True);
        Assert.That(setCookieHeaders.Any(c => c.StartsWith($"{AuthCookieNames.RefreshToken}=")), Is.True);
    }

    [Test]
    public async Task RefreshAsync_GivenNoCookie_WhenCalled_ThenReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/api/v1/auth/refresh", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task LogoutAsync_GivenLoggedInSession_WhenCalledThenRefreshIsRetried_ThenRefreshFails()
    {
        using var client = CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await RegisterAndLoginAsync(client, email);

        var logoutResponse = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.That(logoutResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var refreshAfterLogout = await client.PostAsync("/api/v1/auth/refresh", null);
        Assert.That(refreshAfterLogout.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
