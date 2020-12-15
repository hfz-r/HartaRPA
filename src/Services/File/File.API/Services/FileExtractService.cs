using CsvHelper;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Harta.Services.File.API.Infrastructure.Exceptions;
using IO = System.IO;

namespace Harta.Services.File.API.Services
{
    public class FileExtractService : IFileExtractService
    {
        private readonly ILogger<FileExtractService> _logger;
        private readonly FileSettings _settings;

        public FileExtractService(ILogger<FileExtractService> logger, IOptions<FileSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        #region Private/internal methods

        internal static T TryGetValue<T>(IEnumerable<string> keys, IReaderRow reader)
        {
            foreach (var key in keys)
            {
                var field = reader.GetField(key);
                if (!string.IsNullOrEmpty(field)) return field.GetValueOrNull<T>();

                // try get record from headers by index
                var context = reader.Context;
                if (context.HeaderRecord.Contains(key))
                {
                    var rIndex = context.Record[Array.IndexOf(context.HeaderRecord, key)];
                    if (!string.IsNullOrEmpty(rIndex)) return rIndex.GetValueOrNull<T>();
                }
            }

            return default;
        }

        #endregion

        public async Task ReadFileAsync(string fileName, string systemType)
        {
            var csvFile = IO.Path.Combine(_settings.SourceFolder, fileName);

            if (!IO.File.Exists(csvFile))
            {
                _logger.LogWarning("file={file} not exist in the folder={folder}", fileName, _settings.SourceFolder);

                throw new FileDomainException($"{fileName} not exist.");
            }

            var skip = false;
            var records = new List<PurchaseOrder>();

            try
            {
                using var reader = new IO.StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap<PurchaseOrderMap>();
                csv.Configuration.HeaderValidated = OnHeaderValidated;
                csv.Configuration.MissingFieldFound = null;

                csv.Read();
                csv.ReadHeader();
                csv.ValidateHeader<PurchaseOrder>();

                if (skip) return;

                while (await csv.ReadAsync())
                {
                    var record = new PurchaseOrder
                    {
                        PurchaseOrderDate = csv.TryGetValue<DateTime>(new[] {"Purchase_Order_Date"}),
                        PurchaseOrderNumber = csv.TryGetValue<string>(new[] {"Purchase_Order_Number"}),
                        CompanyName = csv.TryGetValue<string>(new[] {"Company_name"}),
                        RequestedShippedDate = csv.TryGetValue<DateTime?>(new[] {"Requested_Shipped_Date"}),
                        ItemDescription = csv.TryGetValue<string>(new[] {"Table-repeated-section-1:item_description", "item_description"}),
                        Quantity = csv.TryGetValue<int?>(new[] {"quantity"}),
                        ItemNumber = csv.TryGetValue<string>(new[] {"Item_Number"}),
                        UnitOfMeasure = csv.TryGetValue<string>(new[] {"Unit_of_Measure"}),
                        Size = csv.TryGetValue<string>(new[] {"Size"}),
                        MaterialNo = csv.TryGetValue<string>(new[] {"Material_No"}),
                        Result = csv.TryGetValue<string>(new[] {"Result", "item_description"})
                    };

                    records.Add(record);
                }

                await WriteFileAsync(fileName, records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);

                throw;
            }

            #region Local methods

            void OnHeaderValidated(InvalidHeader[] headers, ReadingContext context)
            {
                #region Headers specification for a valid PO

                var headerSpec = new[]
                {
                    "Purchase_Order_Date",
                    "Purchase_Order_Number",
                    "Company_name",
                    "Requested_Shipped_Date",
                    "item_description",
                    "quantity",
                    "Item_Number",
                    "Unit_of_Measure",
                    "Size",
                    "Material_No",
                    "Result"
                };

                #endregion

                if (headers.Length == 0 && context.HeaderRecord.SequenceEqual(headerSpec)) skip = true;
            }

            #endregion
        }

        public async Task WriteFileAsync(string fileName, IList<PurchaseOrder> records)
        {
            var csvFile = IO.Path.Combine(_settings.SourceFolder, fileName.AppendTimeStamp());

            try
            {
                using var writer = new IO.StreamWriter(csvFile) {AutoFlush = true};
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap<PurchaseOrderMap>();

                await csv.WriteRecordsAsync(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);

                throw;
            }
        }
    }

    public static class FileExtractExtensions
    {
        public static T GetValueOrNull<T>(this string src)
        {
            var type = typeof(T);

            return (T) (Nullable.GetUnderlyingType(type) != null
                ? ToNullable<T>(src)
                : Convert.ChangeType(src, typeof(T)));
        }

        public static T TryGetValue<T>(this IReaderRow reader, IEnumerable<string> keys)
        {
            return FileExtractService.TryGetValue<T>(keys, reader);
        }

        public static string AppendTimeStamp(this string fileName)
        {
            return string.Concat(
                IO.Path.GetFileNameWithoutExtension(fileName),
                "_",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                IO.Path.GetExtension(fileName));
        }

        public static T ToNullable<T>(this string src)
        {
            if (string.IsNullOrEmpty(src) || src.Trim().Length <= 0) return default;

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFrom(src);
        }
    }
}