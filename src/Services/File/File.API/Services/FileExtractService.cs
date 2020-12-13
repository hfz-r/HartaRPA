using CsvHelper;
using Harta.Services.File.API.Exceptions;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task ReadFileAsync(string fileName, string systemType)
        {
            var settings = _settings.SourceFile;
            var csvFile = IO.Path.Combine(settings.Folder, fileName);

            if (!IO.File.Exists(csvFile))
            {
                _logger.LogWarning("file={file} not exist in the folder={folder}", fileName, _settings.SourceFile);
                throw new FileDomainException($"{fileName} not exist.");
            }

            try
            {
                //using var reader = new IO.StreamReader(csvFile);
                //using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                //// configuration
                //csv.Configuration.RegisterClassMap<PurchaseOrderMap>();
                //csv.Configuration.HeaderValidated = async (InvalidHeader[] headers, ReadingContext context) =>
                //{
                //    if (headers.Length > 0 || !context.HeaderRecord.SequenceEqual(settings.Headers))
                //    {
                //        await WriteFileAsync();
                //        return;
                //    }
                //};
                //// validate header
                //await csv.ReadAsync();
                //csv.ReadHeader();
                //csv.ValidateHeader<PurchaseOrder>();

                using var reader = new IO.StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                // configuration
                csv.Configuration.RegisterClassMap<PurchaseOrderMap>();
                csv.Configuration.HeaderValidated = ValidateHeader;
                // validate header
                await csv.ReadAsync();
                csv.ReadHeader();
                csv.ValidateHeader<PurchaseOrder>();

                while(await csv.ReadAsync())
                {
                    var row = csv.Context.Row;

                    var result = csv.GetField(0);
                }
            }
            catch (Exception ex)
            {
                if (ex is HeaderValidationException)
                {
                    return;
                }

                throw;
            }

            void ValidateHeader(InvalidHeader[] headers, ReadingContext context)
            {
                for (int i = 0; i < settings.Headers.Length; i++)
                {
                    if (settings.Headers[i] != context.HeaderRecord[i])
                    {
                        context.HeaderRecord[i] = settings.Headers[i];
                    }
                }
            }
        }
    }
}