using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Web;
using Auth.Application.Commands;
using Auth.Application.Commands.FlowRelated;
using Auth.Application.Commands.UserRelated;
using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Presentation.Contracts;
using Microsoft.AspNetCore.Authentication;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Presentation.Endpoints;


public static class LoginRoute
{
    public static void MapLogin(this WebApplication app)
    {
        app.MapGet("/oauth/line", LineLogin).RequireAuthorization("authorize");
        app.MapGet("/oauth/login", Login).RequireAuthorization("authorize");
    }

    private static Task<IResult> LineLogin(HttpContext ctx,HttpRequest req)
    {
        // processing - 檢查是否有 state claim
        var state = ctx.User.FindFirstValue("state");
        if(state == null)
            throw Failure.BadRequest("request /authorize endpoint first");

        // processing - 取得 redirect URI（也就是，如果 Line 認證通過後，在打一次 authorize endpoint）
        var uri = HttpUtility.HtmlDecode(req.QueryString.Value?.Substring(13));

        // processing - 進行 Line OAuth 認證
        return Task.FromResult(Results.Challenge(
            new AuthenticationProperties
            {
                RedirectUri = uri
            }, new List<string>() { "line" }));
    }
    
    private static async Task<IResult> Login(HttpContext ctx, IMediator mediator, LoginRequest request)
    {
        // processing - 檢查是否有 state claim
        var state = ctx.User.FindFirstValue("state");
        if(state == null)
            throw Failure.BadRequest("request /authorize endpoint first");
            
        // processing - 執行登入邏輯
        await mediator.SendAsync(request.ToCommand(state));
            
        // returning -
        //     TODO: 最正確的 OAuth 流程應該是返回 302，But 一直出問題，等找到答案再回頭改
        return Results.Ok();
    }
}

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        // --------------------------------------------------------------------------------
        // Endpoint - 
        //     Line Login
        // --------------------------------------------------------------------------------
        app.MapGet("/oauth/line", (HttpContext ctx,HttpRequest req) =>
        {
            // processing - 檢查是否有 state claim
            var state = ctx.User.FindFirstValue("state");
            if(state == null)
                throw Failure.BadRequest("request /authorize endpoint first");

            // processing - 取得 redirect URI（也就是，如果 Line 認證通過後，在打一次 authorize endpoint）
            var uri = Uri.UnescapeDataString(req.QueryString.Value!.Substring(13));
            
            // processing - 進行 Line OAuth 認證
            return Results.Challenge(new AuthenticationProperties
            {
                RedirectUri = uri
            }, new List<string>() { "line" });
        }).RequireAuthorization("authorize");

        // --------------------------------------------------------------------------------
        // Endpoint - 
        //     Login 
        // --------------------------------------------------------------------------------
        app.MapPost("/oauth/login", async (HttpContext ctx, IMediator mediator, LoginRequest request) =>
        {
            // processing - 檢查是否有 state claim
            var state = ctx.User.FindFirstValue("state");
            if(state == null)
                throw Failure.BadRequest("request /authorize endpoint first");
            
            // processing - 執行登入邏輯
            await mediator.SendAsync(request.ToCommand(state));
            
            // returning -
            //     TODO: 最正確的 OAuth 流程應該是返回 302，But 一直出問題，等找到答案再回頭改
            return Results.Ok();
        }).RequireAuthorization("authorize");
    }
}