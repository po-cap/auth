using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Po.Api.Response;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;

public record struct LoginCommand : IRequest
{
    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; init; }

    /// <summary>
    /// For OAuth Flow to remember who you are
    /// </summary>
    public string State { get; set; }
}

public class LoginHandler : IRequestHandler<LoginCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICodeService _codeService;
    private readonly ISessionRepository _sessionRepository;


    public LoginHandler(
        IUserRepository userRepository, 
        IPasswordService passwordService, 
        ICodeService codeService, 
        ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _codeService = codeService;
        _sessionRepository = sessionRepository;
    }

    public async Task HandleAsync(LoginCommand request)
    {
        var staff = await _userRepository.GetByEmailAsync(request.Email);

        var valid = _passwordService.Validate(
            password: request.Password,
            cipherText: staff.Password ?? throw Failure.BadRequest("User has not password"));
        
        if(!valid)
            throw Failure.BadRequest("Wrong Password");

        _sessionRepository.SetSession(state: request.State, userId: staff.Id);
    }
}