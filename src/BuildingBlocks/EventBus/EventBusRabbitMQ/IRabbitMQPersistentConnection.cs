using System;
using RabbitMQ.Client;

namespace Harta.BuildingBlocks.EventBusRabbitMQ
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}