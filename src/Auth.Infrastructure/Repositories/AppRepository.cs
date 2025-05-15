using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Po.Api.Response;

namespace Auth.Infrastructure.Repositories;

public class AppRepository : IAppRepository
{
    private readonly AppDbContext _dbContext;

    public AppRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 取得 - Application 資訊
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    /// <exception cref="Failure"></exception>
    public async Task<App> GetAppAsync(string clientId)
    {
        var app = await _dbContext.Apps.FirstOrDefaultAsync(x => x.Id == clientId);
        if (app is null)
            throw Failure.NotFound();
        return app;
    }
}