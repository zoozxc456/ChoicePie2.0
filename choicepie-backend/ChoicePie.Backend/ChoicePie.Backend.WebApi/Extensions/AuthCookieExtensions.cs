using ChoicePie.Backend.Shared.Kernel.Auth;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class AuthCookieExtensions
{
    private const string CookiePath = "/api";
    private const int RefreshTokenExpirationDays = 30;

    extension(HttpResponse response)
    {
        public void SetAuthCookies(string accessToken, string refreshToken, int accessTokenExpirationSeconds)
        {
            response.Cookies.Append(AuthCookieNames.AccessToken, accessToken,
                BuildCookieOptions(DateTimeOffset.UtcNow.AddSeconds(accessTokenExpirationSeconds)));

            response.Cookies.Append(AuthCookieNames.RefreshToken, refreshToken,
                BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(RefreshTokenExpirationDays)));
        }

        public void ClearAuthCookies()
        {
            response.Cookies.Delete(AuthCookieNames.AccessToken, BuildCookieOptions(DateTimeOffset.UtcNow));
            response.Cookies.Delete(AuthCookieNames.RefreshToken, BuildCookieOptions(DateTimeOffset.UtcNow));
        }
    }

    private static CookieOptions BuildCookieOptions(DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        // 前後端目前同屬 localhost（不同 port），SameSite 判斷的是 registrable domain 不含 port，Lax 可行；
        // 若正式環境前後端分屬不同網域，需改成 SameSite.None + Secure。
        SameSite = SameSiteMode.Lax,
        Path = CookiePath,
        Expires = expires
    };
}
