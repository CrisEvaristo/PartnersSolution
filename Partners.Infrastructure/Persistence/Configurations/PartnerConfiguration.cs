using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Partners.Domain.Entities;

namespace Partners.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TaxNumber).HasColumnType("varchar(9)").HasMaxLength(9).IsUnicode(false).IsRequired();
        builder.HasIndex(x => x.TaxNumber).IsUnique().HasDatabaseName("IX_Partners_TaxNumber");
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.CreditLimit).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.Version).IsRowVersion().IsConcurrencyToken();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
