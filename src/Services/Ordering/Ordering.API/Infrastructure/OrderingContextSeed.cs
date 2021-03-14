using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Harta.Services.Ordering.API.Extensions;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.SeedWork;
using Harta.Services.Ordering.Infrastructure;
using Harta.Services.Ordering.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Harta.Services.Ordering.API.Infrastructure
{
    public class OrderingContextSeed
    {
        public async Task SeedAsync(IUnitOfWork worker, IWebHostEnvironment environment, IOptions<OrderingSettings> settings, ILogger<OrderingContextSeed> logger, OrderingContext context)
        {
            var policy = CreatePolicy(logger, nameof(OrderingContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = environment.ContentRootPath;

                var customerRepo = worker.GetRepositoryAsync<Customer>();
                var orderStatusRepo = worker.GetRepositoryAsync<OrderStatus>();
                var sysTypeRepo = worker.GetRepositoryAsync<SystemType>();

                await context.Database.MigrateAsync();

                if (!(await customerRepo.GetQueryableAsync()).Any())
                {
                    await customerRepo.AddAsync(useCustomizationData
                        ? GetCustomersFromFile(contentRootPath, logger)
                        : GetPredefinedCustomers());

                    await worker.SaveChangesAsync();
                }

                if (!(await orderStatusRepo.GetQueryableAsync()).Any())
                {
                    await orderStatusRepo.AddAsync(useCustomizationData
                        ? GetOrderStatusFromFile(contentRootPath, logger)
                        : GetPredefinedOrderStatus());

                    await worker.SaveChangesAsync();
                }

                if (!(await sysTypeRepo.GetQueryableAsync()).Any())
                {
                    await sysTypeRepo.AddAsync(useCustomizationData
                        ? GetSystemTypesFromFile(contentRootPath, logger)
                        : GetPredefinedSystemTypes());

                    await worker.SaveChangesAsync();
                }
            });
        }

        #region Private/Protected methods

        protected IEnumerable<Customer> GetPredefinedCustomers()
        {
            return new[]
            {
                new Customer(
                    Guid.NewGuid().ToString(),
                    "H001",
                    "HART",
                    new Address("109B, Jalan SS 21/1a", "Damansara Utama, Petaling Jaya", "Selangor", "Malaysia", "47400"),
                    "HARTALEGA")
            };
        }

        protected IEnumerable<OrderStatus> GetPredefinedOrderStatus()
        {
            return Enumeration.GetAll<OrderStatus>();
        }

        protected IEnumerable<SystemType> GetPredefinedSystemTypes()
        {
            return Enumeration.GetAll<SystemType>();
        }

        private IEnumerable<Customer> GetCustomersFromFile(string contentRootPath, ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "Customers.csv");

            if (!File.Exists(csvFile)) return GetPredefinedCustomers();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<CustomerDto>().ToList();
                return records
                    .SelectTry(x => new Customer(
                        Guid.NewGuid().ToString(),
                        x.AX4CustCode,
                        x.D365CustCode,
                        null,
                        x.CustName))
                    .OnCaughtException(x =>
                    {
                        logger.LogError(exception: x, "EXCEPTION ERROR: {Message}", x.Message);
                        return null;
                    })
                    .Where(x => x != null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                return GetPredefinedCustomers();
            }
        }

        private IEnumerable<OrderStatus> GetOrderStatusFromFile(string contentRootPath, ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "OrderStatus.csv");

            if (!File.Exists(csvFile)) return GetPredefinedOrderStatus();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                int i = 1;
                var records = csv.GetRecords<OrderStatusDto>().ToList();
                return records
                    .SelectTry(x => new OrderStatus(i++, x.OrderStatus))
                    .OnCaughtException(e =>
                    {
                        logger.LogError(e, "EXCEPTION ERROR: {Message}", e.Message);
                        return null;
                    })
                    .Where(x => x != null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                return GetPredefinedOrderStatus();
            }
        }

        private IEnumerable<SystemType> GetSystemTypesFromFile(string contentRootPath, ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "SystemTypes.csv");

            if (!File.Exists(csvFile)) return GetPredefinedSystemTypes();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                int i = 1;
                var records = csv.GetRecords<SystemTypeDto>().ToList();
                return records
                    .SelectTry(x => new SystemType(i++, x.SystemType))
                    .OnCaughtException(e =>
                    {
                        logger.LogError(e, "EXCEPTION ERROR: {Message}", e.Message);
                        return null;
                    })
                    .Where(x => x != null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
                return GetPredefinedSystemTypes();
            }
        }

        private AsyncRetryPolicy CreatePolicy(ILogger<OrderingContextSeed> logger, string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception,
                            "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}",
                            prefix, exception.GetType().Name, exception.Message, retry, retries);
                    }
                );
        }

        #endregion
    }

    #region Data holder class

    internal class CustomerDto
    {
        public string AX4CustCode { get; set; }
        public string D365CustCode { get; set; }
        public string CustName { get; set; }
    }

    internal class OrderStatusDto
    {
        public string OrderStatus { get; set; }
    }

    internal class SystemTypeDto
    {
        public string SystemType { get; set; }
    }

    #endregion
}