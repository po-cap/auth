namespace Auth.Application.Services;

public interface IPasswordService
{
    /// <summary>
    /// 密碼做加密，最後得到的密文是 (Salt + pbkdf2 result) 專換成 Base64String
    /// </summary>
    /// <param name="password">使用者輸入的密碼</param>
    /// <returns></returns>
    string Hash(string password);

    /// <summary>
    /// 驗證使用者輸入的密碼是否正確
    /// </summary>
    /// <param name="password">使用者輸入密碼</param>
    /// <param name="cipherText">密文</param>
    /// <returns></returns>
    bool Validate(string password,string cipherText);

    /// <summary>
    /// 生產一組密碼
    /// </summary>
    /// <param name="default_length"></param>
    /// <returns></returns>
    public string Generate(int default_length = 14);
}