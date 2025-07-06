using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Configurations;

/// <summary>
/// 密鑰
/// </summary>
public class Key
{
    /// <summary>
    /// Default Constructor
    /// </summary>
    public Key() { }

    /// <summary>
    /// 建立 Key
    /// </summary>
    /// <param name="path">Keys 的路徑</param>
    public Key(string path)
    {
        // processing
        //     尋找 KeyPath 底下
        RsaKeys = new List<RsaKey>();
        var rsaFiles = Directory.GetFiles(path, "id_rsa*");
        foreach (var rsaFile in rsaFiles)
        {            
            // processing - 
            var prKey= RSA.Create();
            prKey.ImportRSAPrivateKey(File.ReadAllBytes(rsaFile), out _);
        
            // processing - 
            var puKey = RSA.Create();
            puKey.ImportRSAPublicKey(prKey.ExportRSAPublicKey(), out _);
            
            // processing - 
            var id = rsaFile.Split('.')[^1];

            var rsaKey = new RsaKey()
            {
                ID = id,
                Private = prKey,
                Public = puKey
            };
            
            // processing - 
            RsaKeys.Add(rsaKey);
        }
        
        // description - 
        AesKey = File.ReadAllBytes($"{path}/id_aes");
        
    }
    
    /// <summary>
    /// 非對稱式鑰匙
    /// </summary>
    public List<RsaKey> RsaKeys { get; set; }

    /// <summary>
    /// 對稱式密鑰
    /// </summary>
    public byte[] AesKey { get; init; }

    /// <summary>
    /// 隨機取得一個 Rsa Key
    /// </summary>
    /// <returns></returns>
    public RsaKey RandomGetRsaKey()
    {
        var random = new Random();
        return RsaKeys[random.Next(RsaKeys.Count)];
    }

    /// <summary>
    /// Json Web Keys
    /// </summary>
    public List<JsonWebKey> Jwks
    {
        get
        {
            var jwks = new List<JsonWebKey>();
        
            foreach (var rsaKey in RsaKeys)
            {
                // processing - 取得 Rsa key 中 Public Key 的部分
                var securityKey = new RsaSecurityKey(rsaKey.Public)
                {
                    // Key 的 ID
                    KeyId = rsaKey.ID 
                };
        
                // processing - 建立一個 Json Web Key
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);
                jwk.Alg = "RS256";
                jwk.Use = "sig";
        
                // processing - 加入 Json Web Key List
                jwks.Add(jwk);
            }

            return jwks;    
        }
    }
}

/// <summary>
/// RSA 鑰匙
/// </summary>
public class RsaKey
{
    /// <summary>
    /// ID
    /// </summary>
    public required string ID { get; set; }
    
    /// <summary>
    /// 非對稱式 - 密鑰
    /// </summary>
    public required RSA Private { get; init; }

    /// <summary>
    /// 非對稱式 - 公鑰
    /// </summary>
    public required RSA Public { get; init; }
}


public static class KeyExtension
{
    public static IServiceCollection AddKey(
        this IServiceCollection services, IConfiguration configuration)
    {
        // variable - 
        var directory = Environment.GetEnvironmentVariable("ASPNETCORE_DIRECTORY");
        
        // processing - 
        services.AddSingleton(new Key(directory ?? "./"));
        
        // return 
        return services;
    }
}