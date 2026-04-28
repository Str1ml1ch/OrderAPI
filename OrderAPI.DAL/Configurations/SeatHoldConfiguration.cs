using OrderAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.DAL.Configurations
{
    public class SeatHoldConfiguration : IEntityTypeConfiguration<SeatHold>
    {
        public void Configure(EntityTypeBuilder<SeatHold> entity)
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
        }
    }
}
