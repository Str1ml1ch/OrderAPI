using OrderAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.DAL.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entity)
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.CustomerId).IsRequired();
            entity.Property(o => o.EventId).IsRequired();
            entity.Property(o => o.TotalAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.Currency).IsRequired().HasConversion<string>().HasMaxLength(10);
            entity.Property(o => o.OrderStatus).IsRequired().HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(o => o.CustomerId);
            entity.HasIndex(o => o.EventId);
            entity.HasIndex(o => o.OrderStatus);
            entity.HasIndex(o => o.CreatedAt);

            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.SeatHolds)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.SectionHolds)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
