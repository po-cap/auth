using Auth.Application.Services;
using Auth.Domain.Repositories;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;

public record struct SetPasswordCommand : IRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; set; }
}


public class SetPasswordHandler : IRequestHandler<SetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public SetPasswordHandler(
        IUserRepository userRepository, 
        IPasswordService passwordService, 
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(SetPasswordCommand notification)
    {
        var staff = await _userRepository.GetByIdAsync(notification.UserId);

        var password = _passwordService.Hash(notification.Password);
        staff.Password = password;

        await _unitOfWork.SaveChangeAsync();
    }
}