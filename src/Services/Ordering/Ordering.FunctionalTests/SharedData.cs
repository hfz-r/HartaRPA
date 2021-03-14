using System;
using Google.Protobuf.WellKnownTypes;
using Harta.Services.Ordering.Grpc;

namespace Ordering.FunctionalTests
{
    public static class SharedData
    {
        public static CreateOrderRequest FakeOrderRequest() => new CreateOrderRequest
        {
            Path = "super_fake_2021.csv",
            SystemType = "AX4",
            Type = CreateOrderRequest.Types.Type.P1,
            Orders =
            {
                new OrderDTO
                {
                    Ponumber = "BER47",
                    CustomerRef = "MEDLINE EUROPE",
                    Podate = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    Lines =
                    {
                        new OrderLineDTO
                        {
                            Fgcode = "FG-123-A",
                            Size = "S",
                            Quantity = 200
                        },
                        new OrderLineDTO
                        {
                            Fgcode = "FG-123-XL",
                            Size = "XL",
                            Quantity = 350
                        }
                    }
                },
                new OrderDTO
                {
                    Ponumber = "CP-P05K",
                    CustomerRef = "ABENA A/S",
                    Podate = Timestamp.FromDateTime(DateTime.Now.AddHours(3).ToUniversalTime()),
                    Lines =
                    {
                        new OrderLineDTO
                        {
                            Fgcode = "FG-369-CP",
                            Size = "XXL",
                            Quantity = 690
                        },
                    }
                }
            }
        };
    }
}