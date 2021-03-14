using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Grpc;

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
                systemType: "D365",
                customerRef: "HART");
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
                new Order("test1.csv", "PO-2", DateTime.Now, "D365", "FakeUser1", 1),
                new Order("test2.csv", "PO-1", DateTime.Now, "D365", "FakeUser1", 1)
            };
        }

        public static IEnumerable<Order> FakeOrders2()
        {
            var order1 = new Order("test1.csv", "PO-2", DateTime.Now, "AX4", "FakeUser2", 2);
            order1.AddOrderLine("FG-1", "S", 10);
            order1.AddOrderLine("FG-2", "M", 40);

            var order2 = new Order("test2.csv", "PO-1", DateTime.Now, "AX4", "FakeUser1", 1);
            order2.AddOrderLine("FG-21", "XS", 6);

            return new List<Order> { order1, order2 };
        }

        public static IEnumerable<Customer> FakeCustomers()
        {
            return new List<Customer>
            {
                new Customer("1", "AX41", "D3651", new AddressData().Init()),
                new Customer("2", "AX2", "D3652", new AddressData().Init())
            };
        }

        public static CreateOrderRequest FakeOrderRequest() => new CreateOrderRequest
        {
            Path = "fake.csv",
            SystemType = "D365",
            Type = CreateOrderRequest.Types.Type.P1,
            Orders =
            {
                new OrderDTO
                {
                    Ponumber = "AB111",
                    CustomerRef = "ABC1",
                    Podate = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    Lines =
                    {
                        new OrderLineDTO
                        {
                            Fgcode = "FG1",
                            Size = "S",
                            Quantity = 100
                        },
                        new OrderLineDTO
                        {
                            Fgcode = "FG1",
                            Size = "XL",
                            Quantity = 190
                        }
                    }
                },
                new OrderDTO
                {
                    Ponumber = "CD333",
                    CustomerRef = "CDE3",
                    Podate = Timestamp.FromDateTime(DateTime.Now.AddHours(3).ToUniversalTime()),
                    Lines =
                    {
                        new OrderLineDTO
                        {
                            Fgcode = "FG1",
                            Size = "S",
                            Quantity = 60
                        },
                    }
                }
            }
        };
    }
}