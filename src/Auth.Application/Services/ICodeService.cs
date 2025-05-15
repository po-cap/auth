using Auth.Application.Commands;
using Auth.Application.Models;

namespace Auth.Application.Services;

public interface ICodeService
{
    /// <summary>
    /// 解密 - Authorization Code
    /// </summary>
    /// <param name="cipher"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    Code UnProtectCode(string cipher);

    /// <summary>
    /// 加密 - Authorization Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    string ProtectCode(Code code);

    /// <summary>
    /// 建立 Authorization code
    /// </summary>
    /// <param name="state"></param>
    /// <param name="clientId"></param>
    /// <param name="codeChallenge"></param>
    /// <param name="codeChallengeMethod"></param>
    /// <returns></returns>
    Code CreateCode(
        string state,
        string clientId,
        string codeChallenge,
        string codeChallengeMethod);
}