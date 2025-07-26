namespace Auth.Application.Models;

public record struct RefreshToken
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public required long UserId { get; init; }

    /// <summary>
    /// 過期時間
    /// </summary>
    public required DateTimeOffset Expired { get; init; }
}