using System.Security.Cryptography;
using System.Text;
using Auth.Application.Services;
using Auth.Infrastructure.Utils;

namespace Auth.Infrastructure.Services;

public class CryptoService : ICryptoService
{
    private readonly Key _key;

    public CryptoService(Key key)
    {
        _key = key;
    }

    /// <summary>
    /// 建立 RSA Key
    /// </summary>
    public void CreateRsaKey()
    {
        var rsaKey = RSA.Create();
        var privateKey = rsaKey.ExportRSAPrivateKey();
        File.WriteAllBytes("id_rsa", privateKey);
    }

    /// <summary>
    /// 建立 AES Key
    /// </summary>
    public void CreateAesKey()
    {
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        File.WriteAllBytes("id_aes", key);
    }

    /// <summary>
    /// 明文加密 (AES_GCM)
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public byte[] Encrypt(string plainText)
    {
        // description - 
        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        // description -
        var cipherText = Encoding.UTF8.GetBytes(plainText).AesEncrypt(
            key: _key.AesKey, 
            nonce: nonce);

        // description - 
        return cipherText.Text.Combine(cipherText.Tag).Combine(nonce);
    }

    /// <summary>
    /// 明文解密 (AES_GCM)
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public string Decrypt(byte[] cipherText)
    {
        var nonce = cipherText.SubSet(cipherText.Length-12,cipherText.Length);
        var tag = cipherText.SubSet(cipherText.Length-12-16,cipherText.Length-12);
        var text = cipherText.SubSet(0, cipherText.Length-12-16);
        
        // description - 
        var plainText = text.AesDecrypt(
            key: _key.AesKey,
            nonce: nonce,
            tag: tag);
        
        // description - 
        return Encoding.UTF8.GetString(plainText);
    }
}