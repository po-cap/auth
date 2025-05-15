namespace Auth.Infrastructure.Persistence;

public enum RedisDbEnum
{
    /// <summary>
    /// Default
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// OAuth 的 Code flow 用到的 session
    /// </summary>
    Auth = 1,
    
    /// <summary>
    /// Token Session
    /// </summary>
    Token = 2,
}