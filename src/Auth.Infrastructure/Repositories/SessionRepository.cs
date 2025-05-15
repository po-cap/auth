using System.Security.Cryptography;
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
    /// <param name="userId"></param>
    public Session SetSession(string state, string userId)
    {
        // processing - 
        _redisDb.HashSet($"oauth:{state}", new HashEntry[]
        {
            new ("user_id",userId),
        });

        // processing - 
        _redisDb.KeyExpire(state, TimeSpan.FromMinutes(5));

        // return - 
        return new Session
        {
            State = state,
            UserId = userId
        };
    }
    
    /// <summary>
    /// 從 Session
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public Session? GetSession(string state)
    {
        var userId = _redisDb.HashGet($"oauth:{state}", "user_id");

        if (userId.IsNull) return null;
        
        return new Session()
        {
            State = state,
            UserId = userId.ToString()
        };
    }
}