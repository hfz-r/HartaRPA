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
        private string _path;
        private string _customerReference;
        private string _poNumber;
        private DateTime _poDate;
        private int _systemTypeId;
        private int _orderStatusId;
        private int? _customerId;
        private readonly List<OrderLine> _orderLines;

        public SystemType SystemType { get; private set; }
        public OrderStatus OrderStatus { get; private set; }
        public int? CustomerId => _customerId;
        public IReadOnlyCollection<OrderLine> OrderLines => _orderLines;

        protected Order()
        {
            _orderLines = new List<OrderLine>();
        }

        public Order(string path, string customerReference, string poNumber, DateTime poDate, int systemTypeId,
            string orderId, string orderName, int? customerId = null) : this()
        {
            _path = path;
            _customerReference = customerReference;
            _poNumber = poNumber;
            _poDate = poDate;
            _systemTypeId = systemTypeId;
            _orderStatusId = OrderStatus.Processed.Id;
            _customerId = customerId;

            AddOrderStartedDomainEvent(orderId, orderName);
        }

        #region Private methods

        private void AddOrderStartedDomainEvent(string orderId, string orderName)
        {
            AddDomainEvent(new OrderStartedDomainEvent(orderId, orderName, this));
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

        public SystemType GetSystemType()
        {
            return SystemType.From(_systemTypeId);
        }

        public decimal GetTotal()
        {
            return _orderLines.Sum(x => x.GetQuantity());
        }
    }
}