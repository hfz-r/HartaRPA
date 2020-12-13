using CsvHelper.Configuration;
using System;

namespace Harta.Services.File.API.Model
{
    public class PurchaseOrder
    {
        public DateTime PODate { get; set; }
        public string PONumber { get; set; }
        public string CompanyName { get; set; }
        public DateTime RequestedShippedDate { get; set; }
        public string ItemDescription { get; set; }
        public int Quantity { get; set; }
        public string ItemNumber { get; set; }
        public string UnitOfMeasure { get; set; }
        public string Size { get; set; }
        public string MaterialNo { get; set; }
        public string Result { get; set; }
    }

    public sealed class PurchaseOrderMap : ClassMap<PurchaseOrder>
    {
        public PurchaseOrderMap()
        {
            Map(m => m.PODate).Name("Purchase_Order_Date");
            Map(m => m.PONumber).Name("Purchase_Order_Number");
            Map(m => m.CompanyName).Name("Company_name");
            Map(m => m.RequestedShippedDate).Name("Requested_Shipped_Date");
            Map(m => m.ItemDescription).Name("item_description");
            Map(m => m.Quantity).Name("quantity");
            Map(m => m.ItemNumber).Name("Item_Number");
            Map(m => m.UnitOfMeasure).Name("Unit_of_Measure");
            Map(m => m.Size).Name("Size");
            Map(m => m.MaterialNo).Name("Material_No");
            Map(m => m.Result).Name("Result");
        }
    }
}
