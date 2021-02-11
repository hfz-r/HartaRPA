using AutoMapper;
using Google.Protobuf.Collections;
using Harta.Services.File.API.Model;
using Harta.Services.Ordering.Grpc;

namespace Harta.Services.File.API.Infrastructure.AutoMapper
{
    public partial class RepeatedFieldTypeConverter : ITypeConverter<PurchaseOrder, RepeatedField<OrderLineDTO>>
    {
        public RepeatedField<OrderLineDTO> Convert(PurchaseOrder source, RepeatedField<OrderLineDTO> destination, ResolutionContext context)
        {
            destination ??= new RepeatedField<OrderLineDTO>();
            destination.Add(context.Mapper.Map<OrderLineDTO>(source));

            return destination;
        }
    }

    public partial class RepeatedFieldTypeConverter : ITypeConverter<RepeatedField<OrderLineDTO>, PurchaseOrder>
    {
        public PurchaseOrder Convert(RepeatedField<OrderLineDTO> source, PurchaseOrder destination, ResolutionContext context)
        {
            destination ??= new PurchaseOrder();
            foreach (var item in source)
            {
                destination = context.Mapper.Map<PurchaseOrder>(item);
            }

            return destination;
        }
    }
}