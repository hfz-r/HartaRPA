using System.Reflection;
using AutoMapper;

namespace Harta.Services.Ordering.API.Infrastructure.AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var destinationProperties = typeof(TDestination).GetProperties(flags);

            foreach (var propertyInfo in destinationProperties)
            {
                if (typeof(TSource).GetProperty(propertyInfo.Name, flags) == null)
                {
                    expression.ForMember(propertyInfo.Name, opt => opt.Ignore());
                }
            }

            return expression;
        }
    }
}