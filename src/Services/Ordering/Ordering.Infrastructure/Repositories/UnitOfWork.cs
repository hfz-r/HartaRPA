using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harta.Services.Ordering.Infrastructure.Repositories
{
    public class UnitOfWork<TContext>: IUnitOfWork<TContext> where TContext : DbContext, IDisposable
    {
        private readonly IMediator _mediator;
        private Dictionary<Type, object> _repositories;

        public TContext Context { get; }

        public UnitOfWork(TContext context, IMediator mediator)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>() where TEntity : class, IAggregateRoot
        {
            if (_repositories == null) _repositories = new Dictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type)) _repositories[type] = new RepositoryAsync<TEntity>(Context);
            return (IRepositoryAsync<TEntity>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEventsAsync(Context);
            await Context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}