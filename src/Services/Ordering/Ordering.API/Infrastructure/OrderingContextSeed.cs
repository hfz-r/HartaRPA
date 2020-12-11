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
        public async Task SeedAsync(OrderingContext context, IWebHostEnvironment environment,
            IOptions<OrderingSettings> settings, ILogger<OrderingContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(OrderingContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = environment.ContentRootPath;

                using (context)
                {
                    context.Database.Migrate();

                    if (!context.Customers.Any())
                    {
                        await context.Customers.AddRangeAsync(useCustomizationData
                            ? GetCustomersFromFile(contentRootPath, logger)
                            : GetPredefinedCustomers());

                        await context.SaveChangesAsync();
                    }

                    if (!context.OrderStatus.Any())
                    {
                        await context.OrderStatus.AddRangeAsync(useCustomizationData
                            ? GetOrderStatusFromFile(contentRootPath, logger)
                            : GetPredefinedOrderStatus());

                        await context.SaveChangesAsync();
                    }

                    if (!context.SystemTypes.Any())
                    {
                        await context.SystemTypes.AddRangeAsync(useCustomizationData
                            ? GetSystemTypesFromFile(contentRootPath, logger)
                            : GetPredefinedSystemTypes());

                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        #region Private/Protected methods

        protected IEnumerable<Customer> GetPredefinedCustomers()
        {
            return new[]
            {
                new Customer(Guid.NewGuid().ToString(), "Hartalega",
                    new Address("", "Damansara Utama, Petaling Jaya", "Selangor", "", ""), "HART", "HART")
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

        private IEnumerable<Customer> GetCustomersFromFile(string contentRootPath,
            ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "Customers.csv");

            if (!File.Exists(csvFile)) return GetPredefinedCustomers();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<CustomerDto>();
                return records
                    .SelectTry(x =>
                        new Customer(Guid.NewGuid().ToString(), x.CustName, null, x.AX4CustCode, x.D365CustCode))
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

        private IEnumerable<OrderStatus> GetOrderStatusFromFile(string contentRootPath,
            ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "OrderStatus.csv");

            if (!File.Exists(csvFile)) return GetPredefinedOrderStatus();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                int i = 1;
                var records = csv.GetRecords<OrderStatusDto>();
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

        private IEnumerable<SystemType> GetSystemTypesFromFile(string contentRootPath,
            ILogger<OrderingContextSeed> logger)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "SystemTypes.csv");

            if (!File.Exists(csvFile)) return GetPredefinedSystemTypes();

            try
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                int i = 1;
                var records = csv.GetRecords<SystemTypeDto>();
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