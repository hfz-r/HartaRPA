using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harta.Services.Ordering.Infrastructure.EntityConfigurations
{
    public class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers", OrderingContext.DefaultSchema);
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.DomainEvents);

            builder
                .Property(o => o.Id)
                .UseHiLo("customer-seq", OrderingContext.DefaultSchema);

            builder.OwnsOne(o => o.Address, a => { a.WithOwner(); });

            #region Common properties

            builder.Property(b => b.IdentityGuid)
                .HasMaxLength(200)
                .IsRequired();

            builder
                .HasIndex("IdentityGuid")
                .IsUnique();

            builder.Property(b => b.Name);

            builder
                .Property<string>("_ax4Code")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("AX4Code")
                .IsRequired();

            builder
                .Property<string>("_d365Code")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("D365Code")
                .IsRequired();

            #endregion
        }
    }
}