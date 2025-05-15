using Auth.Application.Commands;
using Auth.Application.Models;

namespace Auth.Presentation.Contracts;

public class TokenResponse
{
    public required string access_token { get; init; }

    public required string refresh_token { get; init; }

    public required string token_type { get; init; }

    public required int expires { get; init; }
}

public static partial class ContractExtension
{
    public static TokenResponse ToResponse(this UserToken tokenResponse)
    {
        return new TokenResponse
        {
            access_token = tokenResponse.AccessToken,
            refresh_token = tokenResponse.RefreshToken,
            token_type = tokenResponse.TokenType,
            expires = tokenResponse.Expires
        };
    }
}