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

            #region General properties

            builder.Property(b => b.IdentityGuid)
                .HasMaxLength(200)
                .IsRequired();

            builder
                .HasIndex("IdentityGuid")
                .IsUnique();

            builder.Property(b => b.AX4Code).IsRequired();
            builder.Property(b => b.D365Code).IsRequired();

            #endregion

            #region Private properties

            builder
                .Property<string>("_name")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Name")
                .IsRequired();
            
            #endregion
        }
    }
}