using System.Security.Claims;
using Auth.Application.Commands.FlowRelated;
using Auth.Presentation.Contracts;
using Microsoft.AspNetCore.Authentication;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Presentation.Endpoints;

public static class LineOAuthEndpoint
{
    public static void MapLineOAuth(this WebApplication app)
    {
        app.MapGet("/oauth/line/authorize",MapLineAuthorize);
    }
    
    /// <summary>
    /// Authorize Endpoint
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="mediator"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private static async Task<IResult> MapLineAuthorize(
        HttpContext ctx, 
        IMediator mediator, 
        [AsParameters] AuthorizationRequest request)
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
            // TODO: 經過代理之後，UseForwardedHeaders，應該是可以直接用 domain name
            var domainName = ctx.Request.Host.Value;
            var domain = Environment.GetEnvironmentVariable("ASPNETCORE_DOMAIN");
                
            // processing - 建立 redirect uri，讓使用者 redirect 到 login page
            // TODO: 變更 Redirect URL 到 /oauth/line?redirectUri=@ViewData["redirectUri"] 
            redirectUrl = new UriBuilder($"https://{domain}/oauth/line")
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
    }
    
}