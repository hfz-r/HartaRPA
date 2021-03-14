using System;
using System.Threading;
using System.Threading.Tasks;
using Harta.BuildingBlocks.EventBus.Extensions;
using Harta.Services.Ordering.API.Application.IntegrationEvents;
using Harta.Services.Ordering.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Harta.Services.Ordering.API.Application.Behaviors
{
    public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly OrderingContext _orderingContext;
        private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public TransactionBehaviour(OrderingContext orderingContext, ILogger<TransactionBehaviour<TRequest, TResponse>> logger, IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _orderingContext = orderingContext ?? throw new ArgumentException(nameof(OrderingContext));
            _logger = logger ?? throw new ArgumentException(nameof(ILogger));
            _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentException(nameof(IOrderingIntegrationEventService));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = default(TResponse);
            var typeName = request.GetGenericTypeName();

            try
            {
                if (_orderingContext.HasActiveTransaction) return await next();

                var strategy = _orderingContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    Guid transactionId;
                    using (var transaction = await _orderingContext.BeginTransactionAsync())
                    using (LogContext.PushProperty("TransactionContext", transaction.TransactionId))
                    {
                        _logger.LogInformation("----- Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);

                        response = await next();

                        _logger.LogInformation("----- Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);

                        await _orderingContext.CommitTransactionAsync(transaction);

                        transactionId = transaction.TransactionId;
                    }

                    await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Handling transaction for {CommandName} ({@Command})", typeName, request);

                throw;
            }
        }
    }
}