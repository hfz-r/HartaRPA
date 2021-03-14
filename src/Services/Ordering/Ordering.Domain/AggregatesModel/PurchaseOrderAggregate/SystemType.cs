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
        public static SystemType D365 = new SystemType(2, nameof(D365));

        public SystemType(int id, string name) : base(id, name)
        {
        }

        public static IEnumerable<SystemType> List() => new[] {AX4, D365};

        public static SystemType From(object val)
        {
            var state = List().SingleOrDefault(x => val switch
            {
                string name => (x.Name == name),
                int id => (x.Id == id),
                _ => throw new ArgumentException()
            });

            return state ?? throw new OrderingDomainException($"Possible values for SystemType: {String.Join(",", List().Select(x => x.Name))}");
        }
    }
}