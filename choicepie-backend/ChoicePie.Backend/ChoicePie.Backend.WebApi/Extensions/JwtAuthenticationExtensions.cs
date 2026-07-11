using System.Text;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChoicePie.Backend.WebApi.Extensions;

public static class JwtAuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddJwtAuthentication(IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            // 用 IOptions<JwtSettings> 而非在註冊當下直接讀 IConfiguration，確保 JwtBearerOptions
            // 是在第一次真正使用時才組出來，讀到的是當時完整合併好的設定（測試用 WebApplicationFactory
            // 覆蓋設定也才吃得到）。
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtSettingsOptions) =>
                {
                    var jwtSettings = jwtSettingsOptions.Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Access token 改用 httpOnly cookie 傳遞，瀏覽器會自動在一般請求與 SignalR
                    // WebSocket handshake 上帶上 cookie。context.Token 沒設值時 JwtBearerHandler
                    // 會 fallback 去讀 Authorization header，保留給非瀏覽器 client 使用。
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken,
                                    out var accessToken) &&
                                !string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}
