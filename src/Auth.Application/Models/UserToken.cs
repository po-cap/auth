namespace Auth.Application.Models;


/// <summary>
/// 回應改使用者的 Token
/// </summary>
public record struct UserToken
{
    /// <summary>
    /// Access Token（將來也會改成加密過的）
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh Token（加密過的）
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token 形式，只會用 Json Web Token 所以這永遠都是 Bearer
    /// </summary>
    public required string TokenType { get; init; }

    /// <summary>
    /// Token 過期時間，會設定成 Refresh Token 的過期時間
    /// </summary>
    public required int Expires { get; init; }
}