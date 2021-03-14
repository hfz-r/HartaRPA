using CsvHelper.Configuration;
using System;

namespace Harta.Services.File.API.Model
{
    public class PurchaseOrder : BaseModel
    {
        public DateTime PurchaseOrderDate { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string CompanyName { get; set; }
        public DateTime? RequestedShippedDate { get; set; }
        public string ItemDescription { get; set; }
        public int? Quantity { get; set; }
        public string ItemNumber { get; set; }
        public string UnitOfMeasure { get; set; }
        public string Size { get; set; }
        public string MaterialNo { get; set; }
        public string Result { get; set; }
        public string Ax4CustomerAcc { get; set; }
        public string CustomerRef { get; set; } 
        public string FgCode { get; set; }
        public string LookupSize { get; set; }
        public string ActualUnit { get; set; }
        public int BaseQty { get; set; }
        public int GlovesInnBoxNo { get; set; }
        public int InnBoxCaseNo { get; set; }
        public int QtyInCase { get; set; }
        public string PoPrefix { get; set; }
    }

    public sealed class ReaderMap : ClassMap<PurchaseOrder>
    {
        public ReaderMap()
        {
            Map(m => m.PurchaseOrderDate).Name("Purchase_Order_Date").TypeConverterOption.Format("dd.MM.yyyy");
            Map(m => m.PurchaseOrderNumber).Name("Purchase_Order_Number");
            Map(m => m.CompanyName).Name("Company_name");
            Map(m => m.RequestedShippedDate).Name("Requested_Shipped_Date").TypeConverterOption.Format("dd.MM.yyyy");
            Map(m => m.ItemDescription).Name("item_description");
            Map(m => m.Quantity).Name("quantity");
            Map(m => m.ItemNumber).Name("Item_Number");
            Map(m => m.UnitOfMeasure).Name("Unit_of_Measure");
            Map(m => m.Size).Name("Size");
            Map(m => m.MaterialNo).Name("Material_No");
            Map(m => m.Result).ConvertUsing(row => row.Result?.ToUpper());
        }
    }

    public sealed class WriterMap : ClassMap<PurchaseOrder>
    {
        public WriterMap(string systemType)
        {
            Map(m => m.PurchaseOrderDate).Name("Purchase_Order_Date").TypeConverterOption.Format("dd.MM.yyyy");
            Map(m => m.PurchaseOrderNumber).Name("Purchase_Order_Number");
            Map(m => m.CompanyName).Name("Company_name");
            Map(m => m.RequestedShippedDate).Name("Requested_Shipped_Date").TypeConverterOption.Format("dd.MM.yyyy");
            Map(m => m.ItemDescription).Name("item_description");
            Map(m => m.Quantity).Name("quantity");
            Map(m => m.ItemNumber).Name("Item_Number");
            Map(m => m.UnitOfMeasure).Name("Unit_of_Measure");
            Map(m => m.Size).Name("Size");
            Map(m => m.MaterialNo).Name("Material_No");
            Map(m => m.Result).Name("Result");

            if (systemType == "AX4") Map(m => m.Ax4CustomerAcc).Name("AX4 Customer Account");
            else Map(m => m.Ax4CustomerAcc).Ignore();

            Map(m => m.CustomerRef).Name("Customer reference");
            Map(m => m.FgCode).Name("FG Code");
            Map(m => m.LookupSize).Name("Lookup Size");
            Map(m => m.ActualUnit).Name("Actual Unit");
            Map(m => m.BaseQty).Name("Base Quantity");
            Map(m => m.GlovesInnBoxNo).Name("Gloves Inner box No");
            Map(m => m.InnBoxCaseNo).Name("Inner box in Case No");
            Map(m => m.QtyInCase).Name("Quantity in Case");
            Map(m => m.PoPrefix).Name("Prefix of PO number");
        }
    }
}