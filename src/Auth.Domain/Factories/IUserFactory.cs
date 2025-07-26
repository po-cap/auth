using Auth.Domain.Entities;

namespace Auth.Domain.Factories;

public interface IUserFactory
{
    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    Task<User> NewAsync(string avatar, string displayName);

    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="oidc">OIDC</param>
    /// <param name="oidcId">OIDC ID</param>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    Task<User> NewAsync(OIDC oidc, string oidcId, string avatar, string displayName);
}