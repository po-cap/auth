using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Domain.Repositories;
using Shared.Mediator.Interface;

namespace Auth.Application.Commands;

public class AuthorizeFlow: IRequest<AuthorizeResponse>
{
    /// <summary>
    /// response_type
    /// </summary>
    public required string ResponseType { get; init; }
    
    /// <summary>
    /// client_id
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// code_challenge
    /// </summary>
    public required string CodeChallenge { get; init; }
    
    /// <summary>
    /// code_challenge_method
    /// </summary>
    public required string CodeChallengeMethod { get; init; }

    /// <summary>
    /// redirect_uri
    /// </summary>
    public required string RedirectUri { get; set; }
    
    /// <summary>
    /// state
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// scopes
    /// </summary>
    public required string[] Scope { get; set; }
}

public class AuthorizeFlowHandler : IRequestHandler<AuthorizeFlow, AuthorizeResponse>
{
    private readonly ICodeService _codeService;
    private readonly ISessionRepository _sessionRepository;

    public AuthorizeFlowHandler(
        ICodeService codeService, 
        ISessionRepository sessionRepository)
    {
        _codeService = codeService;
        _sessionRepository = sessionRepository;
    }


    public Task<AuthorizeResponse> HandleAsync(AuthorizeFlow request)
    {
        // processing - 取得
        var session = _sessionRepository.GetSession(request.State);

        if (session is null)
        {
            throw new Exception();
        }
        
        // processing - 建立 code
        var code = _codeService.CreateCode(
            request.State,
            clientId: request.ClientId, 
            codeChallenge: request.CodeChallenge,
            codeChallengeMethod: request.CodeChallengeMethod);
            
        // processing - 返回 authorize endpoint 需要用到的資訊
        return Task.FromResult(new AuthorizeResponse
        {
            RedirectUrl = request.RedirectUri,
            Code = _codeService.ProtectCode(code),
            State = request.State
        });   
    }
}