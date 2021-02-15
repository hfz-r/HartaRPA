using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harta.Services.Ordering.Infrastructure.EntityConfigurations
{
    public class OrderLineTypeConfiguration : IEntityTypeConfiguration<OrderLine>
    {
        public void Configure(EntityTypeBuilder<OrderLine> builder)
        {
            builder.ToTable("orderlines", OrderingContext.DefaultSchema);
            builder.HasKey(o => o.Id);
            builder.Ignore(o => o.DomainEvents);

            builder
                .Property(o => o.Id)
                .UseHiLo("orderline-seq", OrderingContext.DefaultSchema);

            #region Private properties

            builder.Property<int>("OrderId")
                .IsRequired();

            builder
                .Property<string>("_fgCode")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("FGCode")
                .IsRequired();

            builder
                .Property<string>("_size")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Size")
                .IsRequired();

            builder
                .Property<int>("_quantity")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Quantity")
                .IsRequired();

            #endregion
        }
    }
}