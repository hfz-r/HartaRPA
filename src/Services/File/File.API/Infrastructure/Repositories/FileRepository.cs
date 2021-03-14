using System.Linq;
using System.Threading.Tasks;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Harta.Services.File.API.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IDatabase _database;
        private readonly ILogger<FileRepository> _logger;
        private readonly IConnectionMultiplexer _connection;

        public FileRepository(ILoggerFactory loggerFactory, IConnectionMultiplexer connection)
        {
            _logger = loggerFactory.CreateLogger<FileRepository>();
            _connection = connection;
            _database = connection.GetDatabase();
        }

        #region Private methods

        private IServer GetServer()
        {
            var endpoint = _connection.GetEndPoints();
            return _connection.GetServer(endpoint.FirstOrDefault());
        }

        #endregion

        public async Task<SalesOrder> GetStoreAsync(string poNumber)
        {
            var output = await _database.StringGetAsync(poNumber);
            return !output.IsNullOrEmpty ? JsonConvert.DeserializeObject<SalesOrder>(output) : null;
        }

        public async Task<SalesOrder> UpdateStoreAsync(SalesOrder order)
        {
            var output = await _database.StringSetAsync(order.PONumber, JsonConvert.SerializeObject(order));
            if (!output)
            {
                _logger.LogInformation("Problem occur when persisting the SalesOrder: {@SalesOrder}.", order);
                return null;
            }

            _logger.LogInformation("SalesOrder persisted successfully.");
            return await GetStoreAsync(order.PONumber);
        }

        public async Task<bool> DeleteStoreAsync(string poNumber)
        {
            return await _database.KeyDeleteAsync(poNumber);
        }
    }
}