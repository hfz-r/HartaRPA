using System;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Xunit;

namespace Ordering.UnitTests.Domain
{
    public class CustomerAggregateTest
    {
        [Fact]
        public void Create_customer_success()
        {
            //Arrange    
            var identity = new Guid().ToString();
            var address = new AddressData().Init();

            //Act
            var fakeCustomer = new Customer(identity, "AX41","D3651", address);

            //Assert
            Assert.NotNull(fakeCustomer);
        }

        [Fact]
        public void Create_customer_fail()
        {
            //Arrange    
            var identity = string.Empty;
            var address = new AddressData().Init();

            //Act - Assert
            Assert.Throws<ArgumentNullException>(() => new Customer(identity, "AX41", "D3651", address));
        }
    }
}