using System.Net;
using System.Net.Http.Json;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChoicePie.Backend.WebApi.Tests;

/// <summary>
/// 一般的 SignalR 測試走 LongPolling（TestServer 沒有真正的 Socket，SignalR client 預設的 WebSocket 建不起來）；
/// <see cref="CreateBrowserLikeConnectionAsync"/> 則改用 TestServer 自己的 WebSocket client，模擬前端瀏覽器的連法。
/// Host 端方法需要 JWT（走 AccessToken httpOnly cookie），所以用同一個 CookieContainer
/// 先呼叫 register/login populate cookie，再把同一個 container 交給 HubConnection 用。
/// </summary>
public static class GameHubTestClient
{
    private const string HubPath = "/api/gamehub";

    // CreateAuthenticatedHostClientAsync 打 REST 時用的 origin，cookie 存在這個網址底下。
    private static readonly Uri CookieOrigin = new("https://localhost");

    /// <summary>
    /// 註冊+登入一個 Member，回傳一個帶著已驗證 cookie 的 HttpClient（可拿去打 REST API，例如先建 Quiz），
    /// 以及同一個 CookieContainer，之後可以用 <see cref="CreateHostHubConnectionAsync"/> 建立同一身分的 Hub 連線。
    /// </summary>
    public static async Task<(HttpClient Client, CookieContainer Cookies, string Email)> CreateAuthenticatedHostClientAsync(
        CustomWebApplicationFactory factory)
    {
        var cookieContainer = new CookieContainer();
        var email = $"{Guid.NewGuid()}@example.com";

        var httpClient = new HttpClient(new CookieRelayHandler(cookieContainer, factory.Server.CreateHandler()))
        {
            BaseAddress = CookieOrigin
        };

        await httpClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Name = "Test Host",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        var loginResponse = await httpClient.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = email, Password = "Password123!" });
        loginResponse.EnsureSuccessStatusCode();

        return (httpClient, cookieContainer, email);
    }

    public static async Task<Guid> CreateAndPublishQuizAsync(HttpClient hostClient)
    {
        var createResponse = await hostClient.PostAsJsonAsync("/api/v1/quizzes", new
        {
            Title = "Integration Test Quiz",
            Description = "for GameHub tests",
            CoverEmoji = "🎯",
            CoverGradient = "primary",
            Difficulty = "beginner",
            Tags = new[] { "test" },
            Questions = new[]
            {
                new
                {
                    Text = "1 + 1 = ?",
                    Options = new[] { "1", "2", "3", "4" },
                    AnswerIndex = 1,
                    Explanation = "basic math"
                }
            }
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<QuizDto>>();
        var quizId = created!.Data!.Id;

        var publishResponse = await hostClient.PostAsync($"/api/v1/quizzes/{quizId}/publish", null);
        publishResponse.EnsureSuccessStatusCode();

        return quizId;
    }

    public static async Task<HubConnection> CreateHostHubConnectionAsync(
        CustomWebApplicationFactory factory, CookieContainer cookieContainer)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, HubPath), options =>
            {
                options.HttpMessageHandlerFactory = _ => new CookieRelayHandler(cookieContainer, factory.Server.CreateHandler());
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync();

        return connection;
    }

    /// <summary>便利方法：只需要一個已登入 Host 連線、不需要另外打 REST API 時使用。</summary>
    public static async Task<(HubConnection Connection, string Email)> CreateAuthenticatedHostConnectionAsync(
        CustomWebApplicationFactory factory)
    {
        var (client, cookies, email) = await CreateAuthenticatedHostClientAsync(factory);
        client.Dispose();

        var connection = await CreateHostHubConnectionAsync(factory, cookies);

        return (connection, email);
    }

    public static async Task<HubConnection> CreateAnonymousPlayerConnectionAsync(CustomWebApplicationFactory factory)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, HubPath), options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync();

        return connection;
    }

    /// <summary>
    /// 和前端 useGameRoom 一樣的連法：跳過 negotiate、只用 WebSocket，身分只靠 handshake 帶上的 access_token cookie。
    /// 後端多台時 negotiate 和 WebSocket 可能被分到不同台，所以前端不走 negotiate；這個連法要能在任何一台直接成功。
    /// </summary>
    public static async Task<HubConnection> CreateBrowserLikeConnectionAsync(
        CustomWebApplicationFactory factory, CookieContainer? cookieContainer = null)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, HubPath), options =>
            {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
                options.WebSocketFactory = async (context, ct) =>
                {
                    var webSocketClient = factory.Server.CreateWebSocketClient();
                    var cookieHeader = cookieContainer?.GetCookieHeader(CookieOrigin);
                    if (!string.IsNullOrEmpty(cookieHeader))
                    {
                        webSocketClient.ConfigureRequest = request => request.Headers.Cookie = cookieHeader;
                    }

                    return await webSocketClient.ConnectAsync(context.Uri, ct);
                };
            })
            .Build();

        await connection.StartAsync();

        return connection;
    }

    private sealed class CookieRelayHandler(CookieContainer cookieContainer, HttpMessageHandler inner)
        : DelegatingHandler(inner)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestUri = request.RequestUri!;
            var cookieHeader = cookieContainer.GetCookieHeader(requestUri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var setCookie in setCookieHeaders)
                {
                    cookieContainer.SetCookies(requestUri, setCookie);
                }
            }

            return response;
        }
    }
}
