using Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate;
using Harta.Services.Ordering.Domain.Events;
using Harta.Services.Ordering.Domain.Exceptions;
using Xunit;

namespace Ordering.UnitTests.Domain
{
    public class PurchaseOrderAggregateTest
    {
        [Fact]
        public void Create_order_line_success()
        {
            //Arrange
            var fgCode = "FG-123-GLOVE";
            var size = "XXS"; 
            var quantity = 30;

            //Act
            var fakeOrderLine = new OrderLine(fgCode, size, quantity);

            //Assert
            Assert.NotNull(fakeOrderLine);
        }

        [Fact]
        public void Invalid_order_line_parameters()
        {
            //Arrange
            var fgCode = "FG-123-GLOVE";
            var size = "";
            var quantity = -1;

            //Act - Assert
            Assert.Throws<OrderingDomainException>(() => new OrderLine(fgCode, size, quantity));
        }

        [Fact]
        public void Invalid_quantity_setting()
        {
            //Arrange
            var fgCode = "FG-123-GLOVE";
            var size = "XXS";
            var quantity = 30;

            //Act
            var fakeOrderLine = new OrderLine(fgCode, size, quantity);

            //Assert
            Assert.Throws<OrderingDomainException>(() => fakeOrderLine.AddQuantity(-1));
        }

        [Fact]
        public void Add_two_same_order_invalid()
        {
            var order = new OrderData()
                .Add("FG-123-GLOVE", "XXS", 10)
                .Init();

            Assert.Throws<OrderingDomainException>(() => order.AddOrderLine("FG-123-GLOVE", "XXS", 10));
        }

        [Fact]
        public void Add_new_Order_raises_new_event()
        {
            //Arrange
            var expectedResult = 1;

            //Act
            var fakeOrder = new OrderData().Init();

            //Assert
            Assert.Equal(fakeOrder.DomainEvents.Count, expectedResult);
        }

        [Fact]
        public void Add_event_Order_explicitly_raises_new_event()
        {
            //Arrange
            var expectedResult = 2;

            //Act
            var fakeOrder = new OrderData().Init();
            fakeOrder.AddDomainEvent(new OrderStartedDomainEvent("fakeCustomerName", fakeOrder));

            //Assert
            Assert.Equal(fakeOrder.DomainEvents.Count, expectedResult);
        }

        [Fact]
        public void Remove_event_Order_explicitly()
        {
            //Arrange
            var expectedResult = 1;
            var fakeOrder = new OrderData().Init();
            var @fakeEvent = new OrderStartedDomainEvent("fakeCustomerName", fakeOrder);

            //Act
            fakeOrder.AddDomainEvent(@fakeEvent);
            fakeOrder.RemoveDomainEvent(@fakeEvent);

            //Assert
            Assert.Equal(fakeOrder.DomainEvents.Count, expectedResult);
        }
    }
}