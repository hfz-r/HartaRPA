using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Harta.Services.File.API.Model;
using Microsoft.Extensions.Logging;

namespace Harta.Services.File.API.Infrastructure.Repositories
{
    public class MappingRepository : IMappingRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<MappingRepository> _logger;

        public MappingRepository(IDbConnection dbConnection, ILogger<MappingRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        #region Private methods

        private static dynamic Cast(dynamic obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }

        private static int CalcQty(dynamic obj, int qty) =>
            obj.ActualUnit switch
            {
                "INNER" => qty / Cast(obj.InnBoxCaseNo ?? 0, typeof(int)),
                "PAIR" => qty / (Cast(obj.BaseQty ?? 0, typeof(int)) / 2),
                "POLYBAG" => qty / Cast(obj.InnBoxCaseNo ?? 0, typeof(int)),
                "PCS" => qty / Cast(obj.BaseQty ?? 0, typeof(int)),
                _ => qty
            };

        #endregion

        public async Task<IList<PurchaseOrder>> MapAsync(IEnumerable<PurchaseOrder> payload, string customerRef, string systemType)
        {
            _logger.LogInformation("Fetching mapping records with CustomerRef: {customerRef} and SystemType: {systemType}", customerRef, systemType);

            var sql = @$"
SELECT 
    [CUSTOMER_ARTICLE_CODE_(IN_PO-1)] AS ArticleCode,
    [BRAND_NAME_(IN_PO-1)] AS ItemDesc,
    [FG_CODE_({systemType})] AS FGCode,
    [PO_DOCUMENT_TYPE_(PDF/WORD/EXCEL/MANUAL/PI)] AS DocType,
    {(systemType == "AX4" ? "[CUSTOMER_CODE_(D365)]" : $"[CUSTOMER_CODE_({systemType})]")} AS CustomerRef,
    [CUSTOMER_NAME_(IN_PO)] AS CustomerName,
    [HARTALEGA_SIZE_(IN_SYSTEM)] AS Size,
    [ACTUAL_UNIT_(PCS/PAIR/POLYBAG/INNER/CASE)] AS ActualUnit,
    [Base_Quantity] AS BaseQty,
    [Gloves_Inner_box_No] AS GloveInnBoxNo,
    [Inner_box_in_Case_No] AS InnBoxCaseNo,
    [Prefix_before_PO_Number] AS PoPrefix  
FROM [dbo].[SO_MAPPING] 
WHERE [CUSTOMER_CODE_({systemType})] = @customerRef";

            var result = await _dbConnection.QueryAsync(sql, new {customerRef});

            var output = payload.Select(od =>
            {
                var map = result.FirstOrDefault(res =>
                    !string.IsNullOrEmpty(od.ItemDescription) && !string.IsNullOrEmpty(res.ItemDesc)
                        ? od.ItemDescription.Trim() == Regex.Replace(res.ItemDesc, @"\r\n?|\n", " ").Trim()
                        : !string.IsNullOrEmpty(od.ItemNumber) && !string.IsNullOrEmpty(res.ArticleCode)
                            ? od.ItemNumber == res.ArticleCode
                            : throw new Exception("Mapping failed"));
                if (map != null)
                {
                    od.Ax4CustomerAcc = systemType == "AX4" ? customerRef : string.Empty;
                    od.CustomerRef = map.CustomerRef;
                    od.FgCode = map.FGCode;
                    od.LookupSize = map.Size;
                    od.ActualUnit = map.ActualUnit;
                    od.BaseQty = Cast(map.BaseQty ?? 0, typeof(int));
                    od.GlovesInnBoxNo = Cast(map.GloveInnBoxNo ?? 0, typeof(int));
                    od.InnBoxCaseNo = Cast(map.InnBoxCaseNo ?? 0, typeof(int));
                    od.PoPrefix = map.PoPrefix ?? "0";
                    od.QtyInCase = CalcQty(map, od.Quantity ?? 0);
                }
                return od;
            });

            var mapped =
                output.Where(
                        x => !string.IsNullOrEmpty(x.CustomerRef) && !string.IsNullOrEmpty(x.FgCode))
                    .ToList();

            _logger.LogInformation("Mapping records succeed MapAsync with result : ({@CsvMapped})", mapped);

            return mapped;
        }
    }
}