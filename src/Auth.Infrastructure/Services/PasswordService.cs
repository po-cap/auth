using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Auth.Application.Services;
using Auth.Infrastructure.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Auth.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    /// <summary>
    /// 鹽的Byte數量
    /// </summary>
    public const int SaltBytes = 16;

    /// <summary>
    /// 密文的Byte數量
    /// </summary>
    public const int CipherTextBytes = 32;
    
    /// <summary>
    /// PBKDF2迭代次數
    /// </summary>
    public const int IterationCount = 1000;

    /// <summary>
    /// 哈希函數的選定 
    /// </summary>
    public const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA512;
    
    /// <summary>
    /// 密碼做加密，最後得到的密文是 (Salt + pbkdf2 result) 專換成 Base64String
    /// </summary>
    /// <param name="password">使用者輸入的密碼</param>
    /// <returns></returns>
    public string Hash(string password)
    {
        // processing - Generate Salt 
        var salt = new byte[SaltBytes];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(salt);
        }

        // processing - doing pbkdf2
        var pbkdf2Result = KeyDerivation.Pbkdf2(
            password:password,
            salt: salt,
            prf: Prf,
            iterationCount:IterationCount,
            numBytesRequested: CipherTextBytes);
        
        // processing - combine salt and cipher into one byte array
        var passwordBytes = salt.Combine(pbkdf2Result);

        // processing - transfer the byte array into cipher text
        return Convert.ToBase64String(passwordBytes);

    }

    /// <summary>
    /// 驗證使用者輸入的密碼是否正確
    /// </summary>
    /// <param name="password">使用者輸入密碼</param>
    /// <param name="cipherText">密文</param>
    /// <returns></returns>
    public bool Validate(string password, string cipherText)
    {
        // processing - 
        var passwordBytes = Convert.FromBase64String(cipherText);

        // processing - 
        var salt = passwordBytes.SubSet(startIndex:0, endIndex: SaltBytes);
        
        // processing - 
        var pbkdf2Result = KeyDerivation.Pbkdf2(
            password:password,
            salt: salt,
            prf: Prf,
            iterationCount:IterationCount,
            numBytesRequested: CipherTextBytes);

        // processing - 
        var computeResult = salt.Combine(pbkdf2Result);

        // processing - 
        return ((IStructuralComparable) passwordBytes).CompareTo(computeResult, Comparer<byte>.Default) == 0;
    }

    /// <summary>
    /// 生產一組密碼
    /// </summary>
    /// <param name="default_length"></param>
    /// <returns></returns>
    public string Generate(int default_length = 14)
    {
        const string char_pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
        
        var passwordBuilder = new StringBuilder();
        for (var i = 0; i < default_length; i++)
        {
            // processing - 亂數產生一個 bytes
            var randomByte = RandomNumberGenerator.GetBytes(1);
            // processing - 把亂數利用 mod 鎖定在訂範圍內 (char_length長度內)
            var index = randomByte[0] % char_pool.Length;
            // processing - 第二步鎖定的數變成index，到char_length中取值
            passwordBuilder.Append(char_pool[index]);
        }

        // output - 亂數生產的密碼
        return passwordBuilder.ToString();
    }
}