using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harta.Services.Ordering.Infrastructure.EntityConfigurations
{
    public class SystemTypeEntityTypeConfiguration :IEntityTypeConfiguration<SystemType>
    {
        public void Configure(EntityTypeBuilder<SystemType> builder)
        {
            builder.ToTable("systemtypes", OrderingContext.DefaultSchema);
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasDefaultValue(1)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(o => o.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}