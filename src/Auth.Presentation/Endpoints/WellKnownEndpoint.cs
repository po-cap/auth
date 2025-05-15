using Auth.Infrastructure.Utils;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Presentation.Endpoints;

public static class WellKnownEndpoint
{
    public static void MapWellKnownEndpoint(this IEndpointRouteBuilder app)
    {
        // --------------------------------------------------------------------------------
        // Endpoint- 
        //     Open ID 得配置
        // --------------------------------------------------------------------------------
        app.MapGet("/ouath/.well-known/openid-configuration", (HttpContext ctx,IConfiguration config) =>
        {
            // processing - 取得 Domain Name
            var domainName = ctx.Request.Host.Value;
    
            // return - OpenId Configuration
            return Results.Ok(new
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
            });
        });

        // --------------------------------------------------------------------------------
        // Endpoint - 
        //     Json Web Public Keys
        // --------------------------------------------------------------------------------
        app.MapGet("/oauth/.well-known/jwks", (Key key) =>
        {
            // loop - 
            //     建立 Json Web Key List
            var jwks = new List<JsonWebKey>();
            foreach (var rsaKey in key.RsaKeys)
            {
                // processing - 取得 Rsa key 中 Public Key 的部分
                var securityKey = new RsaSecurityKey(rsaKey.Public)
                {
                    // Key 的 ID
                    KeyId = rsaKey.ID 
                };
        
                // processing - 建立一個 Json Web Key
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);
                jwk.Alg = "RS256";
                jwk.Use = "sig";
        
                // processing - 加入 Json Web Key List
                jwks.Add(jwk);
            }

            // return - Json Web Key List
            return Results.Ok(new
            {
                keys = jwks
            });
        });
    }
    
    
    
}