using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;

public record struct CreateToken : IRequest<UserToken>
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// 認證身份過程用到的 State
    /// </summary>
    public string? State { get; init; }
}


public class CreateTokenHandler : IRequestHandler<CreateToken, UserToken>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ICryptoService _cryptoService;

    public CreateTokenHandler(
        IUserRepository userRepository,
        IJwtService jwtService, 
        ICryptoService cryptoService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _cryptoService = cryptoService;
    }


    public async Task<UserToken> HandleAsync(CreateToken request)
    {
        // Processing - 
        //     為 Json Web Token 建立 jti
        string jti;
        if (request.State is null)
        {
            var bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            jti = Base64UrlTextEncoder.Encode(bytes);
        }
        else
        {
            jti = request.State;
        }
        
        
        // Processing:
        //     建立 Access Token
        var staff = await _userRepository.GetByIdAsync(request.UserId);
        var claims = new List<Claim>()
        {
            new("jti", jti),
            new("sub", staff.Id)
        };
        foreach (var scope in staff.Role.Scopes)
        {
            claims.Add(new("scope", scope.ToString()));
        }
        var accessToken = _jwtService.JsonWebToken(claims, DateTimeOffset.Now.AddMinutes(15));
        
        // Processing:
        //     建立 Refresh Token
        var rawRefreshToken = new RefreshToken
        {
            UserId = request.UserId,
            Expired = DateTimeOffset.Now.AddDays(7)
        };
        var cipher = _cryptoService.Encrypt(JsonSerializer.Serialize(rawRefreshToken));
        var refreshToken = Base64UrlTextEncoder.Encode(cipher);
        
        // Returning:
        //     Token
        return new UserToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            Expires = (int)(rawRefreshToken.Expired - DateTimeOffset.Now).TotalSeconds 
        };
    }
}
