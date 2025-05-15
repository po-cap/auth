using Auth.Domain.Entities;

namespace Auth.Domain.Factories;

public interface IUserFactory
{
    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    Task<User> NewAsync(string id, string avatar, string displayName);
}