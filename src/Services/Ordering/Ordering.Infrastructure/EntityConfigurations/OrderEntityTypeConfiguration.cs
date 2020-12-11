using System;
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

            #region Common properties

            builder
                .Property<string>("_path")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Path")
                .HasMaxLength(200)
                .IsRequired();

            builder
                .Property<string>("_customerReference")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CustomerReference")
                .IsRequired();

            builder
                .Property<string>("_poNumber")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("PONumber")
                .IsRequired();

            builder
                .Property<DateTime>("_poDate")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("PODate")
                .IsRequired();

            #endregion

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
                .Property<int?>("_customerId")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CustomerId")
                .IsRequired(false);

            var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderLines));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            #region Entity relationship

            builder.HasOne(o => o.SystemType)
                .WithMany()
                .HasForeignKey("_systemTypeId");

            builder.HasOne(o => o.OrderStatus)
                .WithMany()
                .HasForeignKey("_orderStatusId");

            builder.HasOne<Customer>()
                .WithMany()
                .IsRequired(false)
                .HasForeignKey("_customerId");

            #endregion
        }
    }
}