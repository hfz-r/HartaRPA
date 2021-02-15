using System.Linq;
using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harta.Services.Ordering.Infrastructure
{
    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync<TContext>(this IMediator mediator, TContext ctx)
            where TContext : DbContext
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(e => e.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent);
        }
    }
}