using System;
using System.Threading;
using System.Threading.Tasks;

namespace Harta.Services.Ordering.Domain.SeedWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>() where TEntity : class, IAggregateRoot;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork<out TContext> : IUnitOfWork
    {
        TContext Context { get; }
    }
}