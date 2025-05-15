using Auth.Domain.Repositories;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;


public record struct UserInfo
{
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
    public string UserId { get; init; }
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

        return new UserInfo()
        {
            Avatar = user.Avatar,
            DisplayName = user.DisplayName
        };
    }
}