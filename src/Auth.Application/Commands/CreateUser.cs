using Auth.Application.Services;
using Auth.Domain.Factories;
using Auth.Domain.Repositories;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;

public record struct CreateUser : IRequest
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public string UserId { get; init; }

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
        var exist = await _userRepository.ExistAsync(request.UserId);

        if (!exist)
        {
            var user = await _userFactory.NewAsync(
                request.UserId, 
                request.Avatar, 
                request.DisplayName);
            _userRepository.Add(user);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}