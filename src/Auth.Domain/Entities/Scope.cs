namespace Auth.Domain.Entities;

public enum Scope
{
    /// <summary>
    /// 超級權限
    /// </summary>
    super,
    
    /// <summary>
    /// 商品中心 - 讀取權限
    /// </summary>
    product_management_reading,
    
    /// <summary>
    /// 商品中心 - 修改權限
    /// </summary>
    product_management_modification
}