using Microsoft.EntityFrameworkCore;
using OrderAPI.DAL.Entities;

namespace OrderAPI.DAL
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<SeatHold> SeatHolds { get; set; }
        public DbSet<SectionHold> SectionHolds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
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
            });

            modelBuilder.Entity<OrderItem>(entity =>
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
            });

            modelBuilder.Entity<SeatHold>(entity =>
            {
                entity.HasKey(sh => sh.Id);

                entity.Property(sh => sh.SeatId).IsRequired(false);
                entity.Property(sh => sh.OderId).IsRequired(false);
                entity.Property(sh => sh.SeatHoldStatus).IsRequired().HasConversion<string>().HasMaxLength(20);

                entity.HasIndex(sh => new { sh.SeatId, sh.SeatHoldStatus });
                entity.HasIndex(sh => sh.HoldExpirationTime);

                entity.HasOne(sh => sh.Order)
                    .WithMany(o => o.SeatHolds)
                    .HasForeignKey(sh => sh.OderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SectionHold>(entity =>
            {
                entity.HasKey(sh => sh.Id);

                entity.Property(sh => sh.SectionId).IsRequired();
                entity.Property(sh => sh.OderId).IsRequired(false);
                entity.Property(sh => sh.SectionHoldStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
                entity.Property(sh => sh.HoldExpirationTime).IsRequired();
                entity.Property(sh => sh.Quantity).IsRequired();

                entity.HasIndex(sh => new { sh.SectionId, sh.SectionHoldStatus });
                entity.HasIndex(sh => sh.HoldExpirationTime);

                entity.HasOne(sh => sh.Order)
                    .WithMany(o => o.SectionHolds)
                    .HasForeignKey(sh => sh.OderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }
    }
}
