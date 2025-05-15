using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;

namespace Auth.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// 取得 - “預設權限”
    /// </summary>
    /// <returns></returns>
    public async Task<Role> GetDefaultAsync()
    {
        var role = await _dbContext.Roles.FindAsync(1);
        if (role == null)
        {
            // TODO: 改一個 Exception 丟
            throw new Exception();
        }

        return role;
    }
}