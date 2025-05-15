using System.Security.Claims;
using Auth.Application.Commands;
using Auth.Presentation.Contracts;
using Microsoft.AspNetCore.Authentication;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Presentation.Endpoints;

public static class OAuthEndpoint
{
    public static void MapOAuthEndpoint(this IEndpointRouteBuilder app)
    {
        // --------------------------------------------------------------------------------
        // Description - 
        //     Authorize Endpoint，主要是給 Frontend 打的
        // --------------------------------------------------------------------------------
        app.MapGet("/oauth/authorize", async (
            HttpContext ctx,
            IMediator mediator, 
            [AsParameters]AuthorizationRequest request) =>
        {
            string redirectUrl;
            // processing - 決定到 callback endpoint
            try
            {
                var command = request.ToCommand();
                var response = await mediator.SendAsync(command);
    
                redirectUrl = new UriBuilder(response.RedirectUrl)
                {
                    Query = QueryString.Create(
                        new Dictionary<string, string?>
                        {
                            ["code"] = response.Code,
                            ["state"] = response.State
                        }).Value
                }.ToString();
                
                await ctx.SignOutAsync("authorize");
            }
            // processing - 決定到 login page
            catch(Exception)
            {
                // processing - 取得 Domain Name
                var domainName = ctx.Request.Host.Value;
                
                // processing - 建立 redirect uri，讓使用者 redirect 到 login page
                redirectUrl = new UriBuilder($"https://{domainName}/oauth/login")
                {
                    Query = QueryString.Create(
                        new Dictionary<string, string?>
                        {
                            ["redirectUri"] = $"{ctx.Request.Path}{ctx.Request.QueryString}"
                        }).Value
                }.ToString();
        
                // processing - 建立 cookie，讓 cookie 存 state 資訊
                await ctx.SignInAsync("authorize", new ClaimsPrincipal(
                    new ClaimsIdentity(new Claim[]
                    {
                        new("state",request.state)
                    }, "authorize")));
            }

            // return - 依據狀態決定要 redirect 到 callback endpoint 還是 login page
            return Results.Redirect(redirectUrl);
        });
        
        // --------------------------------------------------------------------------------
        // Description - 
        //     Token Endpoint，主要是給 Gateway Server 打得，Gateway server 會有一個
        //     Call Back endpoint，Call back endpoint 會來這個 endpoint 要 token
        // --------------------------------------------------------------------------------
        app.MapPost("/oauth/token", async (
            HttpContext ctx,
            IMediator mediator,
            IAuthenticationService authService) =>
        {
            if (ctx.Request.HasFormContentType)
            {
                string grantType, 
                       code, 
                       redirectUri, 
                       codeVerifier, 
                       client_id, 
                       client_secret;
                
                var form = await ctx.Request.ReadFormAsync();
                grantType     = form["grant_type"].ToString();
                code          = form["code"].ToString();
                redirectUri   = form["redirect_uri"].ToString();
                codeVerifier  = form["code_verifier"].ToString();
                client_id     = form["client_id"].ToString();
                client_secret = form["client_secret"].ToString();

                
                var token = await mediator.SendAsync(new TokenFlow
                {
                    GrantType = grantType,
                    Code = code,
                    CodeVerifier = codeVerifier,
                    RedirectUri = redirectUri,
                    ClientId = client_id,
                    ClientSecret = client_secret
                });

                var authorize = await authService.AuthenticateAsync(ctx, "authorize");
                if (authorize.Succeeded)
                {
                    await ctx.SignOutAsync("authorize");
                }
                
                return Results.Ok(token.ToResponse());
            }
            else
            {
                throw Failure.BadRequest();
            }
        });
        
        // --------------------------------------------------------------------------------
        // Description - 
        //     利用 Access Token 來換取使用這資訊
        // --------------------------------------------------------------------------------
        app.MapGet("/oauth/information", async (
            HttpContext ctx,
            IMediator mediator) =>
        {
            var sub = ctx.User.FindFirstValue("sub");
            var command = new GetUserInfo()
            {
                UserId = sub ?? throw Failure.Unauthorized()
            };
            var information = await mediator.SendAsync(command);
            return information;
            
        }).RequireAuthorization("jwt");
    }
}