namespace Auth.Domain.Entities;

public class Role
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 名稱
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 權限
    /// </summary>
    public ICollection<Scope> Scopes { get; set; }
}