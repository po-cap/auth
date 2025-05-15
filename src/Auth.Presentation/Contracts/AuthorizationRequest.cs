using Auth.Application.Commands;

namespace Auth.Presentation.Contracts;

public class AuthorizationRequest
{
    /// <summary>
    /// response_type
    /// </summary>
    public required string response_type { get; init; }
    
    /// <summary>
    /// client_id
    /// </summary>
    public required string client_id { get; init; }
    
    /// <summary>
    /// code_challenge
    /// </summary>
    public required string code_challenge { get; init; }
    
    /// <summary>
    /// code_challenge_method
    /// </summary>
    public required string code_challenge_method { get; init; }

    /// <summary>
    /// redirect_uri
    /// </summary>
    public required string redirect_uri { get; init; }
    
    /// <summary>
    /// state
    /// </summary>
    public required string state { get; init; }

    /// <summary>
    /// Scope
    /// </summary>
    public required string Scope { get; init; }
}

public static partial class ContractExtension
{
    public static AuthorizeFlow ToCommand(this AuthorizationRequest request)
    {
        return new AuthorizeFlow
        {
            ResponseType = request.response_type,
            ClientId = request.client_id,
            CodeChallenge = request.code_challenge,
            CodeChallengeMethod = request.code_challenge_method,
            RedirectUri = request.redirect_uri,
            State = request.state,
            Scope = request.Scope.Trim() != string.Empty ? request.Scope.Split(' ') : new string[0],
        };
    }
}