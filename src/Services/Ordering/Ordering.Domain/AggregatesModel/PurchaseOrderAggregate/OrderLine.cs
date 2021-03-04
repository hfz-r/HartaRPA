using Harta.Services.Ordering.Domain.Exceptions;
using Harta.Services.Ordering.Domain.SeedWork;

namespace Harta.Services.Ordering.Domain.AggregatesModel.PurchaseOrderAggregate
{
    public class OrderLine : Entity
    {
        private string _fgCode;
        private string _size;
        private int _quantity;

        public OrderLine(string fgCode, string size, int quantity)
        {
            _fgCode = !string.IsNullOrWhiteSpace(fgCode)
                ? fgCode
                : throw new OrderingDomainException(nameof(fgCode));

            _size = !string.IsNullOrWhiteSpace(size)
                ? size
                : throw new OrderingDomainException(nameof(size));

            _quantity = quantity >= 0 ? quantity : throw new OrderingDomainException("Invalid number of quantity.");
        }

        public string GetFGCode() => _fgCode;

        public string GetSize() => _size;

        public int GetQuantity() => _quantity;

        public void AddQuantity(int quantity)
        {
            if (quantity < 0)
            {
                throw new OrderingDomainException("Invalid units");
            }

            _quantity += quantity;
        }

        public bool IsEqualTo(string fgCode, string size)
        {
            return _fgCode == fgCode && _size == size;
        }
    }
}