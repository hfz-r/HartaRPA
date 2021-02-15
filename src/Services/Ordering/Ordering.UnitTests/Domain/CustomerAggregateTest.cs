using System;
using Harta.Services.Ordering.Domain.AggregatesModel.CustomerAggregate;
using Xunit;

namespace Ordering.UnitTests.Domain
{
    public class CustomerAggregateTest
    {
        public CustomerAggregateTest()
        {
        }

        [Fact]
        public void Create_customer_success()
        {
            //Arrange    
            var identity = new Guid().ToString();
            var name = "fakeUser";
            var address = new AddressData().Init();

            //Act
            var fakeCustomer = new Customer(identity, name, address);

            //Assert
            Assert.NotNull(fakeCustomer);
        }

        [Fact]
        public void Create_customer_fail()
        {
            //Arrange    
            var identity = string.Empty;
            var name = "fakeUser";
            var address = new AddressData().Init();

            //Act - Assert
            Assert.Throws<ArgumentNullException>(() => new Customer(identity, name, address));
        }
    }
}