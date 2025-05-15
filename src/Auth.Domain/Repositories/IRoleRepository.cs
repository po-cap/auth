using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface IRoleRepository
{
    /// <summary>
    /// 取得 - “預設權限”
    /// 就是 Guest 權限，之後要變更權限，再通知 DBA 進行處理，which is me ==
    /// </summary>
    /// <returns></returns>
    Task<Role> GetDefaultAsync();   
}