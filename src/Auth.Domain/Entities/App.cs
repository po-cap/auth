namespace Auth.Domain.Entities;

public class App
{
    /// <summary>
    /// Client ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Client Secret
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Callback Urls
    /// </summary>
    public List<string> CallbackUrls { get; set; }
}