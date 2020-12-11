using System;
using System.Threading.Tasks;

namespace Harta.Services.Ordering.Infrastructure.Idempotent
{
    public interface IRequestManager
    {
        Task<bool> ExistAsync(Guid id);
        Task CreateRequestForCommandAsync<T>(Guid id);
    }
}