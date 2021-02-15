using System;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;

namespace Ordering.UnitTests
{
    public class AddressData
    {
        public Address Init()
        {
            return new Address("street", "city", "state", "country", "zipcode");
        }
    }

    public class OrderData
    {
        private readonly Order _order;

        public OrderData()
        {
            _order = new Order(
                path: "test.csv",
                poNumber: "HL1/123",
                poDate: DateTime.Now,
                systemTypeId: 1,
                customerId: "Hartalega1",
                customerName: "Hartalega");
        }

        public OrderData Add(string fgCode, string size, int quantity)
        {
            _order.AddOrderLine(fgCode, size, quantity);
            return this;
        }

        public Order Init()
        {
            return _order;
        }
    }
}