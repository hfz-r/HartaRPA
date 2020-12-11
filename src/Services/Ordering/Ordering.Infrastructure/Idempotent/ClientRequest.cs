using System;

namespace Harta.Services.Ordering.Infrastructure.Idempotent
{
    public class ClientRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
    }
}