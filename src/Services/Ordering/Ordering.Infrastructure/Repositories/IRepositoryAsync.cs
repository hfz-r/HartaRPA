using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Harta.Services.Ordering.Infrastructure.Paging;
using Microsoft.EntityFrameworkCore.Query;

namespace Harta.Services.Ordering.Infrastructure.Repositories
{
    public interface IRepositoryAsync<T> : IDisposable where T : class
    {
        Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task AddAsync(params T[] entities);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Delete(IEnumerable<T> entities);
        Task DeleteAsync(object id);
        void Delete(params T[] entities);
        Task DeleteAsync(T entity);
        Task<IPaginate<T>> GetPagedListAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int index = 0, int size = int.MaxValue, bool disableTracking = false, CancellationToken cancellationToken = default);
        Task<IQueryable<T>> GetQueryableAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> queryExp = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false);
        Task<T> SingleAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false);
        void Update(IEnumerable<T> entities);
        void Update(params T[] entities);
        void Update(T entity);
    }
}