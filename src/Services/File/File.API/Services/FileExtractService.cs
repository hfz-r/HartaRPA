using CsvHelper;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static IEnumerable<string> GetKeys(string key, ReadingContext context)
        {
            return context.HeaderRecord.Where(h => h.Contains(key));
        }

        internal static T TryGetValue<T>(string src, IReaderRow reader)
        {
            foreach (var key in GetKeys(src, reader.Context))
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

        public async Task<IEnumerable<PurchaseOrder>> ReadFileAsync(string fileName, string systemType)
        {
            try
            {
                var csvFile = IO.Path.Combine(_settings.SourceFolder, fileName.SetFileName(_settings));

                if (!IO.File.Exists(csvFile))
                {
                    _logger.LogWarning("file={file} not exist in the folder={folder}", fileName,
                        _settings.SourceFolder);

                    throw new FileDomainException($"{fileName} not exist.");
                }

                var records = new List<PurchaseOrder>();

                using var reader = new IO.StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap<ReaderMap>();
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;

                csv.Read();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    if (!string.IsNullOrEmpty(csv.TryGetValue<string>("item_description")) ||
                        !string.IsNullOrEmpty(csv.TryGetValue<string>("Item_Number")))
                    {
                        var record = new PurchaseOrder
                        {
                            PurchaseOrderDate = csv.TryGetValue<DateTime>("Purchase_Order_Date"),
                            PurchaseOrderNumber = csv.TryGetValue<string>("Purchase_Order_Number"),
                            CompanyName = csv.TryGetValue<string>("Company_name"),
                            RequestedShippedDate = csv.TryGetValue<DateTime?>("Requested_Shipped_Date"),
                            ItemDescription = csv.TryGetValue<string>("item_description"),
                            Quantity = csv.TryGetValue<int?>("quantity"),
                            ItemNumber = csv.TryGetValue<string>("Item_Number"),
                            UnitOfMeasure = csv.TryGetValue<string>("Unit_of_Measure"),
                            Size = csv.TryGetValue<string>("Size"),
                            MaterialNo = csv.TryGetValue<string>("Material_No"),
                            Result = csv.TryGetValue<string>("Result"),
                        };

                        records.Add(record);
                    }
                }

                var result =
                    records.Where(rec =>
                        (!string.IsNullOrEmpty(rec.ItemDescription) || !string.IsNullOrEmpty(rec.ItemNumber)) &&
                        rec.Quantity != null);

                _logger.LogInformation("CSV file reader ReadFileAsync succeed with result : ({@CsvReader})", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);

                throw;
            }
        }

        public async Task WriteFileAsync(IList<PurchaseOrder> records, string fileName, string systemType,
            bool timestamp = false)
        {
            if (!records.Any()) throw new ArgumentNullException(nameof(records));

            try
            {
                var csvFile = IO.Path.Combine(_settings.SourceFolder, fileName.SetFileName(_settings).SetTimeStamp(timestamp));

                _logger.LogInformation("Begin to construct mapped results to a new file - {file}", csvFile);

                await using var writer = new IO.StreamWriter(csvFile) {AutoFlush = true};
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap(new WriterMap(systemType));

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
        private static DateTime TryParseDateTime(string src)
        {
            DateTime.TryParseExact(src, new[] {"dd.MM.yyyy", "dd.M.yyyy"}, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var result);
            return result;
        }

        public static T GetValueOrNull<T>(this string src)
        {
            var type = typeof(T);

            return Nullable.GetUnderlyingType(type) != null
                ? ToNullable<T>(src)
                : ChangeType<T>(src);
        }

        public static T TryGetValue<T>(this IReaderRow reader, string key)
        {
            return FileExtractService.TryGetValue<T>(key, reader);
        }

        public static string GetCustomerRef(this string fileName)
        {
            const string pattern = @"(?<=_)\w+(?=_|\.)";

            var r = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = r.Match(fileName);

            if (!match.Success) throw new Exception("Unrecognized filename.");

            var customerRef = match.Value.Contains("_")
                ? match.Value.Split("_")[0]
                : match.Value;

            return customerRef;
        }

        public static string SetFileName(this string fileName, FileSettings settings)
        {
            var ext = IO.Path.GetExtension(fileName);

            return !string.IsNullOrEmpty(ext) && ext == settings.FileExtension
                ? fileName
                : $"{fileName}{settings.FileExtension}";
        }

        public static string SetTimeStamp(this string fileName, bool timestamp)
        {
            return timestamp
                ? string.Concat(
                    IO.Path.GetFileNameWithoutExtension(fileName),
                    "_",
                    DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                    IO.Path.GetExtension(fileName))
                : fileName;
        }

        public static T ChangeType<T>(this string src)
        {
            var convert = typeof(T) == typeof(DateTime)
                ? TryParseDateTime(src)
                : Convert.ChangeType(src, typeof(T), CultureInfo.InvariantCulture);

            return (T) convert;
        }

        public static T ToNullable<T>(this string src)
        {
            if (string.IsNullOrEmpty(src) || src.Trim().Length <= 0) return default;

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFrom(src);
        }
    }
}