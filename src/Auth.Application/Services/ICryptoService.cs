namespace Auth.Application.Services;

public interface ICryptoService
{
    /// <summary>
    /// 建立 RSA Key
    /// </summary>
    void CreateRsaKey();

    /// <summary>
    /// 建立 AES Key
    /// </summary>
    void CreateAesKey();
    
    /// <summary>
    /// 明文加密 (AES_GCM)
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    byte[] Encrypt(string plainText);

    /// <summary>
    /// 明文解密 (AES_GCM)
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public string Decrypt(byte[] cipherText);
}