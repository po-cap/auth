using System.Security.Claims;
using Auth.Application.Services;
using Auth.Infrastructure.Utils;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly Key _key;

    public JwtService(Key key)
    {
        _key = key;
    }
    
    /// <summary>
    /// 建立 JWT Token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="expires"></param>
    /// <returns></returns>
    public string JsonWebToken(List<Claim> claims, DateTimeOffset expires)
    {
        var rsaKey= _key.RandomGetRsaKey();
        
        // description - 
        var key = new RsaSecurityKey(rsaKey.Private)
        {
            KeyId = rsaKey.ID
        };
        
        // description - 
        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
            Expires = expires.DateTime,
            NotBefore = DateTime.UtcNow,
        });

        // return - 
        return token;
    }
}