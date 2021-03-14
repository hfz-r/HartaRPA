using System;
using System.Collections.Generic;
using System.Linq;
using Harta.Services.Ordering.Domain.Events;
using Harta.Services.Ordering.Domain.Exceptions;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate
{
    public class Order : Entity, IAggregateRoot
    {
        private int _systemTypeId;
        private int _orderStatusId;
        private int? _custId;
        private readonly List<OrderLine> _orderLines;

        public string Path { get; }
        public string PONumber { get; }
        public DateTime PODate { get; }
        public SystemType SystemType { get; private set; }
        public OrderStatus OrderStatus { get; private set; }

        public int? GetCustomerId => _custId;
        public IReadOnlyCollection<OrderLine> OrderLines => _orderLines;

        protected Order()
        {
            _orderLines = new List<OrderLine>();
        }

        public Order(string path, string poNumber, DateTime poDate, string systemType, string customerRef, int? custId = null) : this()
        {
            Path = path;
            PONumber = poNumber;
            PODate = poDate;
            _custId = custId; //entity id ref
            _systemTypeId = SystemType.From(systemType).Id;
            _orderStatusId = OrderStatus.Processed.Id;

            AddOrderStartedDomainEvent(customerRef);
        }

        #region Private methods

        private void AddOrderStartedDomainEvent(string customerRef)
        {
            AddDomainEvent(new OrderStartedDomainEvent(customerRef, this));
        }

        #endregion

        public void AddOrderLine(string fgCode, string size, int quantity)
        {
            var existing = _orderLines.SingleOrDefault(x => x.IsEqualTo(fgCode, size));

            if (existing != null) throw new OrderingDomainException("Line existed.");

            var line = new OrderLine(fgCode, size, quantity);
            _orderLines.Add(line);
        }

        public void SetDuplicatedStatus()
        {
            if (_orderStatusId == OrderStatus.Processed.Id)
            {
                AddDomainEvent(new OrderStatusChangedToDuplicateDomainEvent(Id, OrderLines));

                _orderStatusId = OrderStatus.Duplicated.Id;
            }
        }

        public void SetCustomerId(int id) => _custId = id;

        public SystemType GetSystemType() => SystemType.From(_systemTypeId);

        public decimal GetTotal() => _orderLines.Sum(x => x.GetQuantity());
    }
}