using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Auth.Application.Models;
using Auth.Application.Services;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Po.Api.Response;
using StackExchange.Redis;

namespace Auth.Infrastructure.Services;

public class CodeService : ICodeService
{
    private readonly ICryptoService _cryptoService;
    
    public CodeService(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    /// <summary>
    /// 解密 - Authorization Code
    /// </summary>
    /// <param name="cipher"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Code UnProtectCode(string cipher)
    {
        var plainText =  _cryptoService.Decrypt(Convert.FromBase64String(cipher));
        var code = JsonSerializer.Deserialize<Code>(plainText);
        if(code == null)
            throw Failure.BadRequest();
        return code;
    }
    
    /// <summary>
    /// 加密 - Authorization Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public string ProtectCode(Code code)
    {
        var plainText = JsonSerializer.Serialize(code);
        return Convert.ToBase64String(_cryptoService.Encrypt(plainText));
    }
    
    /// <summary>
    /// 建立 Authorization code
    /// </summary>
    /// <param name="state"></param>
    /// <param name="clientId"></param>
    /// <param name="codeChallenge"></param>
    /// <param name="codeChallengeMethod"></param>
    /// <returns></returns>
    public Code CreateCode(
        string state,
        string clientId, 
        string codeChallenge, 
        string codeChallengeMethod)
    {
        return new Code
        {            
            State = state,
            ClientId = clientId,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
        };
    }
    
    /// <summary>
    /// 產生 Code Verifier
    /// Code Verifier 是給 Callback Endpoint 向 token endpoint 發送請求時所攜帶的參數
    /// </summary>
    /// <returns></returns>
    private string GenerateCodeVerifier()
    {
        byte[] bytes = new byte[32]; // RFC 7636建议长度
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlTextEncoder.Encode(bytes);
    }

    /// <summary>
    /// 生產 Code Challenge
    /// Code Challenge 是向 authorize endpoint 發送請求時所攜帶的參數
    /// </summary>
    /// <param name="codeVerifier"></param>
    /// <returns></returns>
    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlTextEncoder.Encode(challengeBytes);
    }
}