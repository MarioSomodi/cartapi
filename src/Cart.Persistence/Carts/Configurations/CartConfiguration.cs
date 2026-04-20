using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Persistence.Carts.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<DomainCart>
{
    public void Configure(EntityTypeBuilder<DomainCart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(cart => cart.TenantId)
            .HasColumnName("TenantId")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(cart => cart.SubjectId)
            .HasColumnName("SubjectId")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(cart => cart.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(cart => cart.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(cart => cart.UpdatedAtUtc)
            .HasColumnName("UpdatedAtUtc")
            .IsRequired();

        builder.Property(cart => cart.Version)
            .HasColumnName("Version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(cart => new { cart.TenantId, cart.SubjectId, cart.Status })
            .IsUnique();

        builder.Navigation(cart => cart.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
