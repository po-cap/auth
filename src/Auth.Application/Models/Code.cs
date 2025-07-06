namespace Auth.Application.Models;

/// <summary>
/// Authorize Endpoint 的 Response
/// </summary>
public class Code
{
    /// <summary>
    /// State
    /// </summary>
    public required string State { get; set; }
    
    /// <summary>
    /// client_id
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// code_challenge
    /// </summary>
    public required string CodeChallenge { get; init; }
    
    /// <summary>
    /// code_challenge_method
    /// </summary>
    public required string CodeChallengeMethod { get; init; }
}