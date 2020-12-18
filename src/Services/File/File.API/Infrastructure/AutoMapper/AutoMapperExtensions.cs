using System;
using System.Reflection;
using AutoMapper;
using Harta.Services.File.API.Model;

namespace Harta.Services.File.API.Infrastructure.AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static TDto ToDto<TDto>(this BaseModel model) where TDto: class
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return AutoMapperConfiguration.Mapper.Map<TDto>(model);
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