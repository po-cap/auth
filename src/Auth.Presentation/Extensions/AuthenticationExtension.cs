using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Auth.Application.Commands.UserRelated;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.Mediator.Interface;

namespace Auth.Presentation.Extensions;

public enum AuthScheme
{
    State,
    Line,
    Jwt,
}

public static class AuthorizeExtension
{
    /// <summary>
    /// Json Web Token 授權
    /// </summary>
    /// <param name="options"></param>
    public static void Jwt(this AuthorizationOptions options)
    {
        options.AddPolicy(AuthScheme.Jwt.GetName(), b =>
        {
            b.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(AuthScheme.Jwt.GetName())
                .RequireClaim("sub");
        });
    }
    
    /// <summary>
    /// Authorize Endpoint 授權
    /// </summary>
    /// <param name="options"></param>
    public static void State(this AuthorizationOptions options)
    {
        options.AddPolicy(AuthScheme.State.GetName(), b =>
        {
            b.RequireAuthenticatedUser()
                .AddAuthenticationSchemes(AuthScheme.State.GetName())
                .RequireClaim("state");
        });
    }
}


public static class AuthenticationExtension
{
    /// <summary>
    /// 取得認證 schema 名稱
    /// </summary>
    /// <param name="scheme"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string GetName(this AuthScheme scheme)
    {
        switch (scheme)
        {
            case AuthScheme.State:
                return "authorize";
            case AuthScheme.Line:
                return "line";
            case AuthScheme.Jwt:
                return "jwt";
            default:
                throw new Exception("No such auth schema");
        }
    }
    
    /// <summary>
    /// Authorize Endpoint 用的身份認證
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static AuthenticationBuilder State(this AuthenticationBuilder builder)
    {
        builder.AddCookie(AuthScheme.State.GetName(), o =>
        {
            o.Cookie.Name = ".authorize";

            o.ExpireTimeSpan = TimeSpan.FromSeconds(240);
            o.Cookie.MaxAge = o.ExpireTimeSpan;
        });

        return builder;
    }
    
    /// <summary>
    /// Line 身份認證
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static AuthenticationBuilder Line(
        this AuthenticationBuilder builder, 
        IConfiguration configuration)
    {
        builder.AddOAuth(AuthScheme.Line.GetName(), o =>
        {
            // Description - 登入後，轉給哪個 schema
            o.SignInScheme = AuthScheme.State.GetName();
            
            // Description - 關於 Server 端配置
            o.ClientId = configuration["Line:ID"] ?? throw new Exception("Please set line id");
            o.ClientSecret = configuration["Line:Secret"] ?? throw new Exception("Please set line secret");

            // Description - 關於 OIDC 端配置
            o.AuthorizationEndpoint = "https://access.line.me/oauth2/v2.1/authorize";
            o.TokenEndpoint = "https://api.line.me/oauth2/v2.1/token";
            o.UserInformationEndpoint = "https://api.line.me/v2/profile";

            // Description - 要向 OIDC 請求什麼 scope
            o.Scope.Clear();
            o.Scope.Add("profile");
            o.Scope.Add("openid");
            o.Scope.Add("email");

            // Description - 關於 Token Saving 的配置
            o.CallbackPath = "/oauth/line-cb";
            o.SaveTokens = true;
            o.Events.OnCreatingTicket = async ctx =>
            {
                // 取得 "authorize" cookie 的 ClaimPrincipal，並且獲取 "state" claim
                var result = await ctx.HttpContext.AuthenticateAsync("authorize");

                if (result.Succeeded && result.Principal != null)
                {
                    // processing - Request User information from line server
                    var req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                    var res = await ctx.Backchannel.SendAsync(req);

                    var state = result.Principal.FindFirstValue("state");

                    if (res.IsSuccessStatusCode)
                    {
                        var user = await res.Content.ReadFromJsonAsync<JsonElement>();
                        var oidcId = user.GetProperty("userId").GetString();

                        var userRepo = ctx.HttpContext.RequestServices.GetService<IUserRepository>();
                        var userInRepo = await userRepo!.GetByOIDCIdAsync(oidcId!);
                        if (userInRepo is null)
                        {
                            var command = new CreateUser()
                            {
                                OIDC = OIDC.line,
                                OIDCId = user.GetProperty("userId").GetString() ?? "",
                                Avatar = user.GetProperty("pictureUrl").GetString() ?? "",
                                DisplayName = user.GetProperty("displayName").GetString() ?? "",
                            };

                            var mediator = ctx.HttpContext.RequestServices.GetService<IMediator>();
                            await mediator!.SendAsync(command);
                        }

                        var sessionRepo = ctx.HttpContext.RequestServices.GetService<ISessionRepository>();
                        sessionRepo?.SetSession(state: state!, oidcId: oidcId!);
                    }
                    else
                    {
                        ctx.Fail("Fail to authenticate");
                    }
                }
                else
                {
                    ctx.Fail("Need .authorize cookie");
                }
            };
        });

        return builder;
    }

    /// <summary>
    /// Json Web Token 身份認證
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static AuthenticationBuilder Jwt(this AuthenticationBuilder builder)
    {
        builder.AddJwtBearer(AuthScheme.Jwt.GetName(), o =>
        {
            // Description - 
            //     告訴 framework，不要把 claim type 變成 Microsoft 自定義的 Type 
            o.MapInboundClaims = false;
            
            // Description - 
            //     定義 openid 的 endpoint 
            var domain = Environment.GetEnvironmentVariable("ASPNETCORE_DOMAIN") 
                         ?? throw new Exception("Set \"ASPNETCORE_DOMAIN\"");
            o.Authority = $"https://{domain}/oauth";
            
            // Description - 
            //     定義 Validate 過程中要 validate 哪些資料
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireExpirationTime = true
            };
            
            
            // TODO: 正式上產品時關掉
            // 啟用詳細錯誤訊息(Debug 用)
            o.IncludeErrorDetails = true;
            
            
            // 事件處理器用於記錄詳細錯誤
            o.Events = new JwtBearerEvents
            {
                // 當認證失敗時
                OnChallenge = async context =>
                {
                    context.HandleResponse();
        
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = context.ErrorDescription ?? "无效的认证令牌",
                        Instance = context.Request.Path
                    };
                
                    var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    problemDetails.Extensions["traceId"] = traceId;
        
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";
        
                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                },
            };
        });
        
        return builder;
    }
}