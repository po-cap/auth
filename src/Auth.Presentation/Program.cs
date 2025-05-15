using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Auth.Application;
using Auth.Application.Commands;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Auth.Infrastructure;
using Auth.Presentation.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Po.Api.Response;
using Scalar.AspNetCore;
using Shared.Mediator.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<LineHttpClient>(c =>
{
    c.BaseAddress = new Uri("https://api.line.me");
});

// 添加  MVC 服務 (支持 Controller + View)
builder.Services.AddControllersWithViews();


builder.Services.AddOpenApi();
builder.Services.AddAuthentication("authorize")
    .AddCookie("authorize", o =>
    {
        o.Cookie.Name = ".authorize";
        
        o.ExpireTimeSpan = TimeSpan.FromSeconds(240);
        o.Cookie.MaxAge = o.ExpireTimeSpan; 
    })
    .AddOAuth("line", o =>
    {
        // Description - 關於 Server 端配置
        o.ClientId     = "2007119716";
        o.ClientSecret = "7f859ffaba9cee6c3815e867a89b6d35";

        // Description - 關於 OIDC 端配置
        o.AuthorizationEndpoint   = "https://access.line.me/oauth2/v2.1/authorize";
        o.TokenEndpoint           = "https://api.line.me/oauth2/v2.1/token";
        o.UserInformationEndpoint = "https://api.line.me/v2/profile";

        // Description - 要向 OIDC 請求什麼 scope
        o.Scope.Clear();
        o.Scope.Add("profile");
        o.Scope.Add("openid");
        o.Scope.Add("email");
        
        // Description - 關於 Token Saving 的配置
        o.CallbackPath            = "/auth/line-cb";
        o.SaveTokens              = true;
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
                    var id = user.GetProperty("userId").GetString();
                    
                    var userRepo  = ctx.HttpContext.RequestServices.GetService<IUserRepository>();
                    var userInRepo = await userRepo.GetByIdAsync(id);
                    if (userInRepo is null)
                    {
                        var command = new CreateUser()
                        {
                            UserId = user.GetProperty("userId").GetString() ?? "",
                            Avatar = user.GetProperty("pictureUrl").GetString() ?? "",
                            DisplayName = user.GetProperty("displayName").GetString() ?? "",
                        };
                        
                        var mediator = ctx.HttpContext.RequestServices.GetService<IMediator>();
                        await mediator.SendAsync(command);
                    }
                    
                    var sessionRepo = ctx.HttpContext.RequestServices.GetService<ISessionRepository>();
                    sessionRepo?.SetSession(state: state, userId: id);
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
    })
    .AddJwtBearer("jwt", o =>
    {
        // Description - 
        //     告訴 framework，不要把 claim type 變成 Microsoft 自定義的 Type 
        o.MapInboundClaims = false;
        
        // Description - 
        //     定義 openid 的 endpoint
        var domain = builder.Configuration["OIDC"];
        o.MetadataAddress = $"{domain}/ouath/.well-known/openid-configuration";
        
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
    });

builder.Services.AddAntiforgery();
builder.Services.AddCors(o =>
{
    o.AddPolicy("line", pb =>
    {
        pb.WithOrigins("https://localhost:5005")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("token", b =>
    {
        b.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("token")
            .RequireClaim("access_token");
    });
    
    o.AddPolicy("jwt", b =>
    {
        b.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("jwt")
            .RequireClaim("sub");
    });
    
    o.AddPolicy("authorize", b =>
    {
        b.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("authorize")
            .RequireClaim("state");
    });
});

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);


var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    
    app.UseStaticFiles();
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();    
    app.UseExceptionHandle();
    app.UseCors("line");
    
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}");
    app.MapWellKnownEndpoint();
    app.MapOAuthEndpoint();
    app.MapLoginEndpoint();

    app.Run();
}
