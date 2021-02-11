using System;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate
{
    public class Customer : Entity, IAggregateRoot
    {
        private string _ax4Code;
        private string _d365Code;

        public string IdentityGuid { get; private set; }
        public string Name { get; private set; }
        public Address Address { get; private set; }

        public Customer(string identity, string name, Address address = null, string ax4Code = "", string d365Code = "")
        {
            _ax4Code = ax4Code;
            _d365Code = d365Code;
            Address = address;
            IdentityGuid = !string.IsNullOrWhiteSpace(identity) ? identity : throw new ArgumentNullException(nameof(identity));
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }
    }
}