using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands.FlowRelated;

public class TokenFlow : IRequest<UserToken>
{
    /// <summary>
    /// 依據這個值，我們能確定要走哪個流程：
    ///     1) authorization_code
    ///     2) refresh_token 
    /// </summary>
    public required string GrantType { get; init; }
    
    /// <summary>
    /// Authorization Code
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// PKCE 流程需要驗證的 Signature
    /// </summary>
    public required string CodeVerifier { get; set; }
    
    /// <summary>
    /// 哪個 Endpoint 呼叫了 token endpoint (callback url)
    /// </summary>
    public required string RedirectUri { get; init; }
    
    /// <summary>
    /// Client ID
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// Client Secret
    /// </summary>
    public required string ClientSecret { get; init; }
    
    /// <summary>
    /// Refresh Token
    /// </summary>
    public required string? RefreshToken { get; init; }
}


public class TokenFlowHandler : IRequestHandler<TokenFlow,UserToken>
{
    private readonly ICodeService _oauthService;
    private readonly IAppRepository _appRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICryptoService _cryptoService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMediator _mediator;

    public TokenFlowHandler(
        ICodeService oauthService, 
        IAppRepository appRepository, 
        IUserRepository userRepository, 
        ICryptoService cryptoService, 
        ISessionRepository sessionRepository, 
        IMediator mediator)
    {
        _oauthService = oauthService;
        _appRepository = appRepository;
        _userRepository = userRepository;
        _cryptoService = cryptoService;
        _sessionRepository = sessionRepository;
        _mediator = mediator;
    }

    private async Task<UserToken> RefreshTokenFlowAsync(TokenFlow request)
    {
        var app = await _appRepository.GetAppAsync(request.ClientId);

        //if (app.Secret != request.ClientSecret)
        //    throw Failure.Unauthorized();
        
        // processing - 
        if (string.IsNullOrEmpty(request.RefreshToken))
            throw Failure.Unauthorized();
        
        var ciphertext = Base64UrlTextEncoder.Decode(request.RefreshToken);
        var plaintext  = _cryptoService.Decrypt(ciphertext);
        var refreshToken= JsonSerializer.Deserialize<RefreshToken>(plaintext);
        
        if(refreshToken.Expired < DateTimeOffset.Now)
            throw Failure.Unauthorized();

        var command = new CreateToken()
        {
            UserId = refreshToken.UserId
        };

        var userToken = await _mediator.SendAsync(command);

        return userToken;
    }
    
    /// <summary>
    /// Authorization Code Flow
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<UserToken> AuthorizationCodeFlowAsync(TokenFlow request)
    {
        var app     = await _appRepository.GetAppAsync(request.ClientId);
        var code    = _oauthService.UnProtectCode(request.Code);
        var session = _sessionRepository.GetSession(code.State);
        
        // processing - 
        if (!ValidateCodeVerifier(code, request.CodeVerifier))
            throw Failure.BadRequest();
        
        // processing - 
        if (code.ClientId != request.ClientId)
            throw Failure.BadRequest();
        
        // processing -
        if (app.Secret != request.ClientSecret)
            throw Failure.BadRequest();
        
        //// processing - 
        //var uri = new Uri(request.RedirectUri); ;
        //if (!app.CallbackUrls.Contains(uri.ToString())) 
        //    throw Failure.BadRequest();
        
        if(session is null)
            throw Failure.BadRequest();
        
        // processing - 
        var user = await _userRepository.GetByIdAsync(session.Value.UserId);
        if (user is null) throw Failure.Unauthorized();
        
        // processing - 
        var command = new CreateToken()
        {
            UserId = user.Id
        };
        return await _mediator.SendAsync(command);
        
    }

    public Task<UserToken> HandleAsync(TokenFlow request)
    {
        switch (request.GrantType)
        {
            case "authorization_code":
                return AuthorizationCodeFlowAsync(request);
            case "refresh_token":
                return RefreshTokenFlowAsync(request);
            default:
                throw Failure.Unauthorized();
        }
    }


    public bool ValidateCodeVerifier(Code code, string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var codeChallenge = Base64UrlTextEncoder.Encode(sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier)));
        return code.CodeChallenge == codeChallenge;
    }
}