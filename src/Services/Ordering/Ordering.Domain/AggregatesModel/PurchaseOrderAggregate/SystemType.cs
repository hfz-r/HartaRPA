using System;
using System.Collections.Generic;
using System.Linq;
using Harta.Services.Ordering.Domain.Exceptions;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate
{
    public class SystemType : Enumeration
    {
        public static SystemType AX4 = new SystemType(1, nameof(AX4));
        public static SystemType D65 = new SystemType(2, nameof(D65));

        public SystemType(int id, string name) : base(id, name)
        {
        }

        public static IEnumerable<SystemType> List() => new[] {AX4, D65};

        public static SystemType From(int id)
        {
            var state = List().SingleOrDefault(x => x.Id == id);

            return state ?? throw new OrderingDomainException(
                $"Possible values for SystemType: {String.Join(",", List().Select(x => x.Name))}");
        }
    }
}