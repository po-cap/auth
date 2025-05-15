using Auth.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Contracts;

public record struct TokenRequest
{
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? grant_type { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? code { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? code_verifier { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? redirect_uri { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? client_id { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    [FromForm]
    public string? client_secret { get; init; }
}

public static partial class ContractExtension
{
    public static TokenFlow ToCommand(this TokenRequest request)
    {
        return new TokenFlow
        {
            GrantType = request.grant_type,
            Code = request.code,
            CodeVerifier = request.code_verifier,
            RedirectUri = request.redirect_uri,
            ClientId = request.client_id,
            ClientSecret =request.client_secret
        };
    }
}