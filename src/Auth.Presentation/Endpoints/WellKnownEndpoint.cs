using Auth.Infrastructure.Configurations;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Presentation.Endpoints;

public static class WellKnownRoutes
{
    public static void MapWellKnownRoutes(this WebApplication app)
    {
        app.MapGet("/ouath/.well-known/openid-configuration", Configuration);
        app.MapGet("/oauth/.well-known/jwks", GetJwks);
    }

    /// <summary>
    /// 取得 OpenID 配置
    /// </summary>
    /// <param name="ctxAccessor"></param>
    /// <returns></returns>
    private static Task<IResult> Configuration(IHttpContextAccessor ctxAccessor)
    {
        // processing - 取得 Domain Name
        var domainName = ctxAccessor.HttpContext?.Request.Host.Value;

        // return - OpenId Configuration
        return Task.FromResult(Results.Ok(new
        {
            issuer                                = $"https://{domainName}",
            authorization_endpoint                = $"https://{domainName}/ouath/authorize",
            token_endpoint                        = $"https://{domainName}/ouath/token",
            userinfo_endpoint                     = $"https://{domainName}/ouath/information",
            jwks_uri                              = $"https://{domainName}/oauth/.well-known/jwks",
            response_types_supported              = new[] { "code" },
            subject_types_supported               = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" },
            scopes_supported                      = new[] { "openid" },
            claims_supported                      = new[] { "sub" },
            grant_types_supported                 = new[] { "authorization_code", "client_credentials" }                
        }));
    }

    /// <summary>
    /// 取得 Json Web Keys
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static Task<IResult> GetJwks(Key key)
    {
        return Task.FromResult(Results.Ok(new
        {
            keys = key.Jwks
        }));
    }
}