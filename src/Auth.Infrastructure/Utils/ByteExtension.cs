namespace Auth.Infrastructure.Utils;

public static class ByteExtension
{
    /// <summary>
    /// Combine two byte array 
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static byte[] Combine(this byte[] first, byte[] second)
    {
        var rv = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, rv, 0, first.Length);
        Buffer.BlockCopy(second, 0, rv, first.Length, second.Length);
        return rv; 
    }

    /// <summary>
    /// Get the subset of the byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex">不包含</param>
    /// <returns></returns>
    public static byte[] SubSet(this byte[] bytes, int startIndex = 0, int endIndex = 0)
    {
        return bytes.Skip(startIndex).Take(endIndex - startIndex).ToArray();
    }
    
    
    /// <summary>
    /// string 轉成 bytes
    /// </summary>
    /// <param name="text">String Format</param>
    /// <returns>Binary Format</returns>
    public static byte[] StringToBytes(this string text)
    {
        return System.Text.Encoding.UTF8.GetBytes(text);
    }
    
    /// <summary>
    /// bytes 轉成 string
    /// </summary>
    /// <param name="bytes">Binary Format</param>
    /// <returns>String Format</returns>
    public static string BytesToString(this byte[] bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}