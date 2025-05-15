using Auth.Domain.Entities;

namespace Auth.Domain.Repositories;

public interface ISessionRepository
{
    /// <summary>
    /// 設定 Code Session
    /// </summary>
    /// <param name="state"></param>
    /// <param name="userId"></param>
    Session SetSession(string state, string userId);

    /// <summary>
    /// 從 Session
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    Session? GetSession(string state);
}