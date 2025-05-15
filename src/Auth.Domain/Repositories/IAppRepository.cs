using Auth.Domain.Entities;
using Po.Api.Response;

namespace Auth.Domain.Repositories;

public interface IAppRepository
{
    /// <summary>
    /// 取得 - Application 資訊
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    /// <exception cref="Failure"></exception>
    Task<App> GetAppAsync(string clientId);
}