using Auth.Domain.Repositories;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands.UserRelated;


public record struct UserInfo
{
    /// <summary>
    /// 使用者 - ID
    /// </summary>
    public required long Id { get; set; }
    
    /// <summary>
    /// 使用者 - 頭像
    /// </summary>
    public required string Avatar { get; set; }

    /// <summary>
    /// 使用者 - 顯示名稱
    /// </summary>
    public required string DisplayName { get; set; }
}


public record struct GetUserInfo : IRequest<UserInfo>
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public long UserId { get; init; }
}

public class GetUserInfoHandler : IRequestHandler<GetUserInfo, UserInfo>
{
    private readonly IUserRepository _userRepository;

    public GetUserInfoHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<UserInfo> HandleAsync(GetUserInfo request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if(user == null)
            throw Failure.Unauthorized();
            
        return new UserInfo()
        {
            Id = user.Id,
            Avatar = user.Avatar,
            DisplayName = user.DisplayName
        };
    }
}