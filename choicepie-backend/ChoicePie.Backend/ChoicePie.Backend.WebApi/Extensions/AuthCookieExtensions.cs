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
        }
    }

    private static CookieOptions BuildCookieOptions(HttpResponse response, DateTimeOffset expires)
    {
        // Secure cookie 只有透過 HTTPS 連線瀏覽器才會存下來；本地開發前後端都是純 HTTP（localhost 不同 port），
        // 若寫死 Secure=true 會導致 cookie 完全存不進去、一 refresh 就消失。正式環境（Staging/Production）
        // 一律走 HTTPS，維持 Secure=true。
        var isDevelopment = response.HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            // 前後端目前同屬 localhost（不同 port），SameSite 判斷的是 registrable domain 不含 port，Lax 可行；
            // 若正式環境前後端分屬不同網域，需改成 SameSite.None + Secure。
            SameSite = SameSiteMode.Lax,
            Path = CookiePath,
            Expires = expires,
            // Domain 沒指定時瀏覽器會用 request host（含 localhost）本身，開發/測試環境才收得到 cookie；
            // 正式環境固定在 minjie.demo，讓子網域（例如 api.minjie.demo）也能共用同一顆 cookie。
            Domain = isDevelopment ? null : "minjie.demo"
        };
    }
}
