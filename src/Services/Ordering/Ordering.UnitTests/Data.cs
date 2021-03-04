using System;
using System.Collections.Generic;
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

    internal static class FakeData
    {
        public static IEnumerable<Order> FakeOrders1()
        {
            return new List<Order>
            {
                new Order("test1.csv", "PO-2", DateTime.Now, 1, "1", "FakeUser1", 1),
                new Order("test2.csv", "PO-1", DateTime.Now, 1, "1", "FakeUser1", 1)
            };
        }

        public static IEnumerable<Order> FakeOrders2()
        {
            var order1 = new Order("test1.csv", "PO-2", DateTime.Now, 1, "2", "FakeUser2", 2);
            order1.AddOrderLine("FG-1", "S", 10);
            order1.AddOrderLine("FG-2", "M", 40);

            var order2 = new Order("test2.csv", "PO-1", DateTime.Now, 1, "1", "FakeUser1", 1);
            order2.AddOrderLine("FG-21", "XS", 6);

            return new List<Order> { order1, order2 };
        }

        public static IEnumerable<Customer> FakeCustomers()
        {
            return new List<Customer>
            {
                new Customer("1", "FakeUser1", new AddressData().Init()),
                new Customer("2", "FakeUser2", new AddressData().Init())
            };
        }
    }
}