using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Harta.Services.File.API.Model;
using Harta.Services.Ordering.Grpc;

namespace Harta.Services.File.API.Infrastructure.AutoMapper
{
    public partial class RepeatedFieldTypeConverter : ITypeConverter<List<PurchaseOrder>, IList<OrderDTO>>
    {
        public IList<OrderDTO> Convert(List<PurchaseOrder> source, IList<OrderDTO> destination, ResolutionContext context)
        {
            var grp = source
                .GroupBy(x => new {x.PurchaseOrderNumber, x.PurchaseOrderDate, x.CustomerRef},
                    (key, g) => new
                    {
                        key.PurchaseOrderNumber,
                        key.PurchaseOrderDate,
                        key.CustomerRef,
                        Orders = g.ToList()
                    });

            destination ??= new List<OrderDTO>();
            foreach (var src in grp)
            {
                var order = new OrderDTO
                {
                    Ponumber = src.PurchaseOrderNumber,
                    Podate = Timestamp.FromDateTime(src.PurchaseOrderDate.ToUniversalTime()),
                    CustomerRef = src.CustomerRef,
                    Lines =
                    {
                        src.Orders
                            .Where(x => !string.IsNullOrEmpty(x.FgCode))
                            .Select(x => x.Map<OrderLineDTO>())
                    }
                };

                destination.Add(context.Mapper.Map<OrderDTO>(order));
            }

            return destination;
        }
    }
}