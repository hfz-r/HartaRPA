using AutoMapper;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Harta.Services.File.API.Model;
using Harta.Services.Ordering.Grpc;

namespace Harta.Services.File.API.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PurchaseOrder, OrderDTO>()
                .IgnoreAllNonExisting()
                .ForMember(d => d.Podate, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.PurchaseOrderDate.ToUniversalTime())))
                .ForMember(d => d.Ponumber, opt => opt.MapFrom(src => src.PurchaseOrderNumber))
                .ForMember(d => d.Lines, opt => opt.MapFrom(src => src))
                .ReverseMap()
                .ForPath(s => s.PurchaseOrderDate, opt => opt.MapFrom(src => src.Podate.ToDateTime()));
            
            CreateMap<PurchaseOrder, RepeatedField<OrderLineDTO>>().ConvertUsing<RepeatedFieldTypeConverter>();
            CreateMap<RepeatedField<OrderLineDTO>, PurchaseOrder>().ConvertUsing<RepeatedFieldTypeConverter>();

            CreateMap<PurchaseOrder, OrderLineDTO>()
                .IgnoreAllNonExisting()
                .ForMember(d => d.Size, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Size) ? src.Size : "" ))
                .ForMember(d => d.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ReverseMap();
        }
    }
}