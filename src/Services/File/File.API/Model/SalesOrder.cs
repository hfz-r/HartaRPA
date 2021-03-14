using System.Collections.Generic;

namespace Harta.Services.File.API.Model
{
    public class SalesOrder : BaseModel
    {
        public string PONumber { get; set; }
        public string CustomerRef { get; set; }
        public List<SalesLine> Lines { get; set; }
    }

    public class SalesLine
    {
        public string FGCode { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
    }
}