using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// ID 是否已經被使用過了
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> ExistAsync(string id);

    /// <summary>
    /// 新增 - “工作人員”
    /// </summary>
    /// <param name="user"></param>
    void Add(User user);

    /// <summary>
    /// 取得 ”工作人員“ By ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<User?> GetByIdAsync(string id);

    /// <summary>
    /// 取得 ”工作人員“ By Email
    /// </summary>
    /// <returns></returns>
    Task<User> GetByEmailAsync(string email);
}