using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate
{
    public class Customer : Entity, IAggregateRoot
    {
        private string _name;

        public string IdentityGuid { get; private set; }
        public string AX4Code { get; private set; }
        public string D365Code { get; private set; }
        public Address Address { get; private set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        protected Customer()
        {
        }

        public Customer(string identity, string ax4Code, string d365Code, Address address, string name = "") : this()
        {
            IdentityGuid = identity;
            AX4Code = ax4Code;
            D365Code = d365Code;
            Address = address;
            _name = name;
        }
    }
}