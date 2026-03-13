using FiapX.Infrastructure.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.ModelsConfiguration;

[ExcludeFromCodeCoverage]
public class UserDbModelConfiguration : IEntityTypeConfiguration<UserDbModel>
{
    public void Configure(EntityTypeBuilder<UserDbModel> entity)
    {
        entity.ToTable("Users");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");

        entity.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.HasIndex(x => x.Email)
            .IsUnique();
    }
}
