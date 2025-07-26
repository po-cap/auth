using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Factories;
using Auth.Domain.Repositories;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands.UserRelated;

public record struct CreateUser : IRequest
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public string OIDCId { get; init; }

    /// <summary>
    /// 身份認證伺服器
    /// </summary>
    public OIDC OIDC { get; set; }
    
    /// <summary>
    /// 頭像
    /// </summary>
    public string Avatar { get; init; }

    /// <summary>
    /// 顯示名稱
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; init; }
}

public class CreateUserHandler : IRequestHandler<CreateUser>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFactory _userFactory;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserHandler(
        IUserRepository userRepository, 
        IUserFactory userFactory, 
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreateUser request)
    {
        var user = await _userFactory.NewAsync(
            oidc: request.OIDC,
            oidcId: request.OIDCId, 
            avatar: request.Avatar, 
            displayName: request.DisplayName);
        _userRepository.Add(user);
        await _unitOfWork.SaveChangeAsync();
    }
}