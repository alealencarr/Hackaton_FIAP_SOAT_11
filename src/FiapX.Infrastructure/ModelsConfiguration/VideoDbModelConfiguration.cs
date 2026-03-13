using FiapX.Infrastructure.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.ModelsConfiguration;

[ExcludeFromCodeCoverage]
public class VideoDbModelConfiguration : IEntityTypeConfiguration<VideoDbModel>
{
    public void Configure(EntityTypeBuilder<VideoDbModel> entity)
    {
        entity.ToTable("Videos");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.OriginalFileName)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(x => x.Status)
            .IsRequired();

        entity.Property(x => x.FrameCount);

        entity.Property(x => x.ZipPath)
            .HasMaxLength(1000);

        entity.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.Property(x => x.ProcessedAt);

        entity.HasOne(x => x.User)
            .WithMany(u => u.Videos)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.Status);
    }
}
