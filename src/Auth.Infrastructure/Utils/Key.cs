using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Auth.Infrastructure.Utils;

/// <summary>
/// 密鑰
/// </summary>
public class Key
{
    /// <summary>
    /// 非對稱式鑰匙
    /// </summary>
    public required List<RsaKey> RsaKeys { get; set; }

    /// <summary>
    /// 對稱式密鑰
    /// </summary>
    public required byte[] AesKey { get; init; }

    /// <summary>
    /// 隨機取得一個 Rsa Key
    /// </summary>
    /// <returns></returns>
    public RsaKey RandomGetRsaKey()
    {
        var random = new Random();
        return RsaKeys[random.Next(RsaKeys.Count)];
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
    public static IServiceCollection AddKey(this IServiceCollection services, IConfiguration configuration)
    {
        // description - 取得有密鑰的路徑
        var path = configuration["KeyPath"] ?? "./";
        //var config = configuration.GetSection("OpenId");
        //if (config != null)
        //{
        //    path = config["KeyPath"] ?? "./";
        //    if (path.Last() == '/')
        //    {
        //        path = path[..^1];
        //    }
        //}
        
        // description - 取得 RSA Keys
        var rsaKeys = new List<RsaKey>();
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
            rsaKeys.Add(rsaKey);
        }
        
        // description - 
        var aesKey = File.ReadAllBytes($"{path}/id_aes");

        // description - 
        var key = new Key()
        {
            RsaKeys = rsaKeys,
            AesKey = aesKey
        };

        // processing - 
        services.AddSingleton(key);
        
        // return 
        return services;
    }
}