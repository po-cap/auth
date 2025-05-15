namespace Auth.Application.Models;

public class AuthorizeResponse
{
    /// <summary>
    /// 也就是 callback endpoint
    /// </summary>
    public required string RedirectUrl { get; init; }

    /// <summary>
    /// code
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// state
    /// </summary>
    public required string State { get; init; }
}