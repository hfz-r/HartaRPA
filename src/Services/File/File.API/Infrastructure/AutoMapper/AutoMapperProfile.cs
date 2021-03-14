using AutoMapper;
using Harta.Services.File.API.Model;
using Harta.Services.Ordering.Grpc;
using System.Collections.Generic;
using Harta.Services.File.API.IntegrationEvents.Events;

namespace Harta.Services.File.API.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<List<PurchaseOrder>, IList<OrderDTO>>().ConvertUsing<RepeatedFieldTypeConverter>();
           
            CreateMap<PurchaseOrder, OrderLineDTO>()
                .IgnoreAllNonExisting()
                .ForMember(d => d.Fgcode, opt => opt.MapFrom(src => src.FgCode))
                .ForMember(d => d.Size, opt => opt.MapFrom(src => src.LookupSize))
                .ForMember(d => d.Quantity, opt => opt.MapFrom(src => src.QtyInCase))
                .ReverseMap();

            CreateMap<OrderStartedIntegrationEvent, SalesOrder>()
                .IgnoreAllNonExisting()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.PONumber, opt => opt.MapFrom(src => src.PONumber))
                .ForMember(d => d.CustomerRef, opt => opt.MapFrom(src => src.CustomerRef))
                .ForMember(d => d.Lines, opt => opt.MapFrom(src => src.Lines))
                .ReverseMap();

            CreateMap<Line, SalesLine>().ReverseMap();
        }
    }
}