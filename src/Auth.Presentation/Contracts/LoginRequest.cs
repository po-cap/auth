using Auth.Application.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Contracts;

public record struct LoginRequest
{
    /// <summary>
    /// /authorize endpoint 加上參數
    /// </summary>
    [FromQuery]
    public string RedirectUrl { get; init; }
    
    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; init; }
}

public static partial class ContractExtension
{
    public static LoginCommand ToCommand(this LoginRequest request, string state)
    {
        return new LoginCommand()
        {
            Email = request.Email,
            Password = request.Password,
            State = state
        };
    }
}