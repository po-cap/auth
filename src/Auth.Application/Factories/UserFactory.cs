using Auth.Domain.Entities;
using Auth.Domain.Factories;
using Auth.Domain.Repositories;

namespace Auth.Application.Factories;

public class UserFactory : IUserFactory
{
    private readonly IRoleRepository _roleRepository;

    public UserFactory(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }
    
    /// <summary>
    /// 建立新用戶
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <param name="avatar">頭像</param>
    /// <param name="displayName">顯示名稱</param>
    /// <returns></returns>
    public async Task<User> NewAsync(string id, string avatar, string displayName)
    {
        var role = await _roleRepository.GetDefaultAsync();

        var staff = new User()
        {
            Id = id,
            Avatar = avatar,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.Now,
            Role = role
        };

        return staff;
    }
}