using OrderAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderAPI.DAL.Configurations
{
    public class SectionHoldConfiguration : IEntityTypeConfiguration<SectionHold>
    {
        public void Configure(EntityTypeBuilder<SectionHold> entity)
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
        }
    }
}
