using Auth.Domain.Entities;
using Auth.Domain.Factories;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Services;

namespace Auth.Infrastructure.Factories;

public class UserFactory : IUserFactory
{
    private readonly SnowflakeID _snowflake;
    private readonly IRoleRepository _roleRepository;

    public UserFactory(SnowflakeID snowflake, IRoleRepository roleRepository)
    {
        _snowflake = snowflake;
        _roleRepository = roleRepository;
    }
    
    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    public async Task<User> NewAsync(string avatar, string displayName)
    {
        var role = await _roleRepository.GetDefaultAsync();
        var id   = _snowflake.Get();
        
        var staff = new User()
        {
            Id = id,
            OIDC = OIDC.xiao_hong_mao,
            OIDCId = id.ToString(),
            Avatar = avatar,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.Now,
            Role = role
        };

        return staff;
    }

    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="oidc">OIDC</param>
    /// <param name="oidcId">OIDC ID</param>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    public async Task<User> NewAsync(OIDC oidc, string oidcId, string avatar, string displayName)
    {
        var role = await _roleRepository.GetDefaultAsync();
        var id   = _snowflake.Get();
        
        var staff = new User()
        {
            Id = id,
            OIDC = oidc,
            OIDCId = oidcId,
            Avatar = avatar,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.Now,
            Role = role
        };

        return staff;
    }
}