using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Po.Api.Response;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// ID 是否已經被使用過了
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(string id)
    {
        return await _dbContext.Staffs.FindAsync(id) != null;
    }

    /// <summary>
    /// 新增 - “工作人員”
    /// </summary>
    /// <param name="staff"></param>
    public void Add(User user)
    {
        _dbContext.Staffs.Add(user);
    }

    /// <summary>
    /// 取得 ”工作人員“ By ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User?> GetByIdAsync(string id)
    {
        return await _dbContext.Staffs
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// 取得 ”工作人員“ By Email
    /// </summary>
    /// <returns></returns>
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _dbContext.Staffs
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email) ?? throw Failure.NotFound();
    }
    
}