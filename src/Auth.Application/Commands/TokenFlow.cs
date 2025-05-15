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

namespace Auth.Application.Commands;

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
}


public class TokenFlowHandler : IRequestHandler<TokenFlow,UserToken>
{
    private readonly ICodeService _oauthService;
    private readonly IAppRepository _appRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ICryptoService _cryptoService;
    private readonly ISessionRepository _sessionRepository;

    public TokenFlowHandler(
        ICodeService oauthService, 
        IAppRepository appRepository, 
        IUserRepository userRepository, 
        IJwtService jwtService, 
        ICryptoService cryptoService, 
        ISessionRepository sessionRepository)
    {
        _oauthService = oauthService;
        _appRepository = appRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _cryptoService = cryptoService;
        _sessionRepository = sessionRepository;
    }


    public async Task<UserToken> HandleAsync(TokenFlow request)
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
        var staff = await _userRepository.GetByIdAsync(session.Value.UserId);
        if (staff is null) throw Failure.NotFound();
        
        // processing - 
        var claims = new List<Claim>()
        {
            new("sub", staff.Id)
        };
        foreach (var scope in staff.Role.Scopes)
        {
            claims.Add(new("scope", scope.ToString()));
        }
        var accessToken = _jwtService.JsonWebToken(claims, DateTimeOffset.Now.AddMinutes(300));

        // processing - 
        var rawRefreshToken = new RefreshToken
        {
            UserId = session.Value.UserId,
            Expired = DateTimeOffset.Now.AddDays(7)
        };
        var cipher = _cryptoService.Encrypt(JsonSerializer.Serialize(rawRefreshToken));
        var refreshToken = Base64UrlTextEncoder.Encode(cipher);

        // returning - 
        return new UserToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            Expires = (int)(rawRefreshToken.Expired - DateTimeOffset.Now).TotalSeconds 
        };
    }


    public bool ValidateCodeVerifier(Code code, string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var codeChallenge = Base64UrlTextEncoder.Encode(sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier)));
        return code.CodeChallenge == codeChallenge;
    }
}