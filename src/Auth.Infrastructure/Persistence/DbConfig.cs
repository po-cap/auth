using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence;

public class DbConfig : 
    IEntityTypeConfiguration<Role>,
    IEntityTypeConfiguration<User>,
    IEntityTypeConfiguration<App>
{
 
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles").HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Scopes).HasColumnName("scopes").HasColumnType("scope[]");          
    }
    
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users").HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Avatar).HasColumnName("avatar");
        builder.Property(x => x.DisplayName).HasColumnName("display_name");    
        builder.Property(x => x.Password).HasColumnName("password");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz");
        
        builder.HasOne<Role>(x => x.Role).WithMany().HasForeignKey("role_id");
    }

    public void Configure(EntityTypeBuilder<App> builder)
    {
        builder.ToTable("applications");
        builder.Property(x => x.Id).HasColumnName("client_id");
        builder.Property(x => x.Secret).HasColumnName("client_secret");
        builder.Property(x => x.CallbackUrls).HasColumnName("callback_urls").HasColumnType("text[]");
    }
}