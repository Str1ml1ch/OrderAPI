using OrderAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.DAL.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> entity)
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.OrderId).IsRequired();
            entity.Property(oi => oi.SectionId).IsRequired();
            entity.Property(oi => oi.SeatId).IsRequired(false);
            entity.Property(oi => oi.Price).IsRequired().HasPrecision(18, 2);

            entity.HasIndex(oi => oi.OrderId);
            entity.HasIndex(oi => oi.SeatId);
            entity.HasIndex(oi => oi.SectionId);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
