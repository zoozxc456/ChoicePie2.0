using ChoicePie.Backend.Shared.Kernel.Auth;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class AuthCookieExtensions
{
    private const string CookiePath = "/";
    private const int RefreshTokenExpirationDays = 30;

    extension(HttpResponse response)
    {
        public void SetAuthCookies(string accessToken, string refreshToken, int accessTokenExpirationSeconds)
        {
            response.Cookies.Append(AuthCookieNames.AccessToken, accessToken,
                BuildCookieOptions(response, DateTimeOffset.UtcNow.AddSeconds(accessTokenExpirationSeconds)));

            response.Cookies.Append(AuthCookieNames.RefreshToken, refreshToken,
                BuildCookieOptions(response, DateTimeOffset.UtcNow.AddDays(RefreshTokenExpirationDays)));
        }

        public void ClearAuthCookies()
        {
            response.Cookies.Delete(AuthCookieNames.AccessToken, BuildCookieOptions(response, DateTimeOffset.UtcNow));
            response.Cookies.Delete(AuthCookieNames.RefreshToken, BuildCookieOptions(response, DateTimeOffset.UtcNow));

            // Delete() 的 Domain/Path 必須與當初 Set-Cookie 時完全一致才能命中：Auth:CookieDomain 這個設定
            // 是後來才加上的，改版前登入的使用者瀏覽器裡還留著 Domain 沒指定（走 request host）時期發出的
            // 舊 cookie，只清新設定值的那組會讓舊 cookie 永遠殘留，導致同名 cookie 重複、後端讀到不確定是
            // 哪一顆。這裡额外清一次沒有 Domain 的版本，確保新舊 cookie 都被移除。
            var legacyOptions = new CookieOptions { Path = CookiePath, Expires = DateTimeOffset.UtcNow };
            response.Cookies.Delete(AuthCookieNames.AccessToken, legacyOptions);
            response.Cookies.Delete(AuthCookieNames.RefreshToken, legacyOptions);
        }
    }

    private static CookieOptions BuildCookieOptions(HttpResponse response, DateTimeOffset expires)
    {
        var services = response.HttpContext.RequestServices;

        // CookieDomain 設定值代表「前後端是否跨子網域部署」，優先於 IsDevelopment() 判斷：
        // ASPNETCORE_ENVIRONMENT=Development 只代表 appsettings 分層與除錯選項，不代表部署拓樸——
        // 共用的 dev/staging 環境即使是 Development environment，前後端也可能分屬不同子網域
        // （例如 choicepie-dev.xxx / choicepie-dev-api.xxx），此時仍需要 Domain=父網域，cookie
        // 才能在 SSR 請求時被瀏覽器帶上；純本機 localhost 開發則不設定，走 request host 的預設行為。
        var cookieDomain = services.GetRequiredService<IConfiguration>()["Auth:CookieDomain"];
        var isCrossOrigin = !string.IsNullOrEmpty(cookieDomain);

        // Secure cookie 只有透過 HTTPS 連線瀏覽器才會存下來；跨子網域時 SameSite 必須是 None，
        // 而 SameSite=None 的 cookie 瀏覽器強制要求 Secure=true，否則會直接整顆丟棄不存。
        // 純本機 localhost 開發前後端都是純 HTTP（不同 port），維持 Secure=false 才存得進去。
        var isDevelopment = services.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
        var isSecure = isCrossOrigin || !isDevelopment;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            // 前後端同屬 localhost（不同 port）時，SameSite 判斷的是 registrable domain 不含 port，Lax 可行；
            // 前後端分屬不同網域（含跨子網域的共用 dev/staging）時，需改成 SameSite.None + Secure。
            SameSite = isCrossOrigin ? SameSiteMode.None : SameSiteMode.Lax,
            Path = CookiePath,
            Expires = expires,
            // Domain 沒指定時瀏覽器會用 request host 本身；設定 CookieDomain 後固定在父網域，
            // 讓子網域（例如 api.minjie.demo）也能共用同一顆 cookie。
            Domain = cookieDomain
        };
    }
}
