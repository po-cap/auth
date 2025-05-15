using System.Security.Cryptography;

namespace Auth.Infrastructure.Utils;

/// <summary>
/// AES-GCM 密文
/// AES-GCM : Advanced Encryption Standard - Galois/Counter Mode
/// </summary>
internal class CipherText
{
    /// <summary>
    /// 密文
    /// </summary>
    public required byte[] Text { get; set; }

    /// <summary>
    /// 簽名(16個 bytes)
    /// </summary>
    public required byte[] Tag { get; set; }
}

internal static class AesGcmExtension
{
    /// <summary>
    /// AES-GCM 加密
    /// </summary>
    /// <param name="plaintext">名文</param>
    /// <param name="key">密鑰(32個 bytes)</param>
    /// <param name="nonce">類似 salt(12個 bytes)</param>
    /// <returns>密文（包括簽名 Tag）</returns>
    public static CipherText AesEncrypt(this byte[] plaintext, byte[] key, byte[] nonce)
    {
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[16]; // 通常 16 字节的认证标签

        using (var aesGcm = new AesGcm(key,16))
        {
            aesGcm.Encrypt(
                nonce: nonce,
                plaintext: plaintext,
                ciphertext: ciphertext,
                tag: tag
            );
        }

        return new CipherText()
        {
            Text = ciphertext,
            Tag = tag
        };
    }
    
    /// <summary>
    /// AES-GCM 解碼
    /// </summary>
    /// <param name="ciphertext">密文</param>
    /// <param name="key">密鑰</param>
    /// <param name="nonce">類似 salt</param>
    /// <param name="tag">密文簽名</param>
    /// <returns>明文</returns>
    public static byte[] AesDecrypt(this byte[] ciphertext, byte[] key, byte[] nonce, byte[] tag)
    {
        byte[] plaintext = new byte[ciphertext.Length];

        using (var aesGcm = new AesGcm(key,16))
        {
            aesGcm.Decrypt(
                nonce: nonce,
                ciphertext: ciphertext,
                tag: tag,
                plaintext: plaintext
            );
        }

        return plaintext;
    }
    
}