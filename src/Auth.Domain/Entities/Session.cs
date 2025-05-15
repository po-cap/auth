namespace Auth.Domain.Entities;

public record struct Session
{
    /// <summary>
    /// State
    /// State 有兩種功能，一種是設計紀錄使用者，另外一種是防止跨站攻擊，
    /// 目前，我們的設計運用了第一種，第二種沒有用到 
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// 使用者 ID，在登入成功後，要建立一個 session，把使用者ID和State映射起來
    /// </summary>
    public required string UserId { get; init; }
}