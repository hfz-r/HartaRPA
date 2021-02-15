using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harta.Services.Ordering.Infrastructure.EntityConfigurations
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", OrderingContext.DefaultSchema);
            builder.HasKey(o => o.Id);
            builder.Ignore(o => o.DomainEvents);

            builder
                .Property(o => o.Id)
                .UseHiLo("order-seq", OrderingContext.DefaultSchema);

            #region General properties

            builder.Property(o => o.PONumber)
                .IsRequired();

            builder.Property(o => o.PODate)
                .IsRequired();

            #endregion

            #region Private properties

            builder
                .Property<string>("_path")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Path")
                .HasMaxLength(200)
                .IsRequired();

            builder
                .Property<int>("_systemTypeId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("SystemTypeId")
                .IsRequired();

            builder
                .Property<int>("_orderStatusId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("OrderStatusId")
                .IsRequired();

            builder
                .Property<int?>("_custId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CustomerId")
                .IsRequired(false);

            #endregion

            #region Entity relationship

            var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderLines));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(o => o.SystemType)
                .WithMany()
                .HasForeignKey("_systemTypeId");

            builder.HasOne(o => o.OrderStatus)
                .WithMany()
                .HasForeignKey("_orderStatusId");

            builder.HasOne<Customer>()
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("_custId");

            #endregion
        }
    }
}