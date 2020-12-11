using Harta.Services.Ordering.Infrastructure.Idempotent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harta.Services.Ordering.Infrastructure.EntityConfigurations
{
    public class ClientRequestEntityTypeConfiguration : IEntityTypeConfiguration<ClientRequest>
    {
        public void Configure(EntityTypeBuilder<ClientRequest> builder)
        {
            builder.ToTable("requests", OrderingContext.DefaultSchema);
            builder.HasKey(cr => cr.Id);
            builder.Property(cr => cr.Name).IsRequired();
            builder.Property(cr => cr.Time).IsRequired();
        }
    }
}