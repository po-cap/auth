using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Po.Api.Response;
using StackExchange.Redis;

namespace Auth.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly IDatabase _redisDb;
    
    public SessionRepository(IConnectionMultiplexer multiplexer)
    {
        _redisDb = multiplexer.GetDatabase((int)RedisDbEnum.Auth);
    }
    
    /// <summary>
    /// 設定 Code Session
    /// </summary>
    /// <param name="state"></param>
    /// <param name="oidcId"></param>
    public Session SetSession(string state, string oidcId)
    {
        // processing - 
        _redisDb.HashSet($"oauth:{state}", new HashEntry[]
        {
            new ("oidc_id",oidcId),
        });

        // processing - 
        _redisDb.KeyExpire(state, TimeSpan.FromMinutes(5));

        // return - 
        return new Session
        {
            State = state,
            oidcId = oidcId
        };
    }
    
    /// <summary>
    /// 從 Session
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public Session? GetSession(string state)
    {
        var oidcId = _redisDb.HashGet($"oauth:{state}", "oidc_id");

        if (oidcId.IsNull) return null;
        
        return new Session()
        {
            State = state,
            oidcId = oidcId
        };
    }
}