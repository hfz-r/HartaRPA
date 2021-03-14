using System.Collections.Generic;
using Harta.BuildingBlocks.EventBus.Events;

namespace Harta.Services.Ordering.API.Application.IntegrationEvents.Events
{
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public string PONumber { get; }
        public string CustomerRef { get; }
        public IEnumerable<Line> Lines { get; }

        public OrderStartedIntegrationEvent(string poNumber, string customerRef, IEnumerable<Line> lines)
        {
            PONumber = poNumber;
            CustomerRef = customerRef;
            Lines = lines;
        }
    }

    public class Line
    {
        public string FGCode { get; }
        public string Size { get; }
        public int Quantity { get; }

        public Line(string fgCode, string size, int quantity)
        {
            FGCode = fgCode;
            Size = size;
            Quantity = quantity;
        }
    }
}