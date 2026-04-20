using Cart.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Persistence.Carts.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(item => item.CartId)
            .HasColumnName("CartId")
            .IsRequired();

        builder.Property(item => item.Sku)
            .HasColumnName("Sku")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasColumnName("Name")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.Currency)
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.HasIndex(item => new { item.CartId, item.Sku })
            .IsUnique();

        builder.HasOne<DomainCart>()
            .WithMany(cart => cart.Items)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
