using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Web;
using Auth.Application.Commands;
using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Presentation.Contracts;
using Microsoft.AspNetCore.Authentication;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Presentation.Endpoints;


public class LineHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IMediator _mediator;

    public LineHttpClient(HttpClient httpClient, IMediator mediator)
    {
        _httpClient = httpClient;
        _mediator = mediator;
    }

    public async Task<UserToken> AuthorizeAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/v2/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            // processing - 
            var user = await response.Content.ReadFromJsonAsync<JsonElement>();
            var createUserCmd = new CreateUser()
            {
                UserId = user.GetProperty("userId").ToString(),
                Avatar = user.GetProperty("pictureUrl").ToString(),
                DisplayName = user.GetProperty("displayName").ToString(),
            };
            await _mediator.SendAsync(createUserCmd);
            
            // processing - 
            var createTokenCmd = new CreateToken()
            {
                UserId = user.GetProperty("userId").ToString()
            };
            var userToken = await _mediator.SendAsync(createTokenCmd);

            // return - 
            return userToken;
        }
        else
        {
            throw Failure.Unauthorized();
        }
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

            // processing - 取得 redirect URI（也就是，如果 Line 認證通過後，在打一次 authorize eendpoint）
            var uri = HttpUtility.HtmlDecode(req.QueryString.Value?.Substring(13));
            
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