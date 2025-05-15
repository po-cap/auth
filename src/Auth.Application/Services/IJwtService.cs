using System.Security.Claims;

namespace Auth.Application.Services;

public interface IJwtService
{
    /// <summary>
    /// 建立 JWT Token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="expires"></param>
    /// <returns></returns>
    string JsonWebToken(List<Claim> claims, DateTimeOffset expires);
}