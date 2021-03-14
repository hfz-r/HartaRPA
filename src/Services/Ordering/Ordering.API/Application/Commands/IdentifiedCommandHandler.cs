using System;
using System.Threading;
using System.Threading.Tasks;
using Harta.BuildingBlocks.EventBus.Extensions;
using Harta.Services.Ordering.Infrastructure.Idempotent;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Commands
{
    public class IdentifiedCommandHandler<T, R> : IRequestHandler<IdentifiedCommand<T, R>, R> where T : IRequest<R>
    {
        private readonly IMediator _mediator;
        private readonly IRequestManager _requestManager;
        private readonly ILogger<IdentifiedCommandHandler<T, R>> _logger;

        public IdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<IdentifiedCommandHandler<T, R>> logger)
        {
            _mediator = mediator;
            _requestManager = requestManager;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static (string commandProperty, string commandId) FromCommand(T command)
        {
            string prop, id;

            switch (command)
            {
                case CreateOrderCommand createOrder:
                    prop = nameof(createOrder.Request.Path);
                    id = createOrder.Request.Path;
                    break;
                default:
                    prop = "Id?";
                    id = "n/a";
                    break;
            }

            return (prop, id);
        }

        protected virtual R CreateResultForDuplicateRequest()
        {
            return default;
        }

        public async Task<R> Handle(IdentifiedCommand<T, R> message, CancellationToken cancellationToken)
        {
            var exists = await _requestManager.ExistAsync(message.Id);
            if (exists) return CreateResultForDuplicateRequest();

            await _requestManager.CreateRequestForCommandAsync<T>(message.Id);
            try
            {
                var command = message.Command;
                var commandName = command.GetGenericTypeName();
                var (commandProperty, commandId) = FromCommand(command);

                _logger.LogInformation(
                    "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    commandName,
                    commandProperty,
                    commandId,
                    command);

                // Send to the actual Handler 
                var result = await _mediator.Send(command, cancellationToken);

                _logger.LogInformation(
                    "----- Command result: {@Result} - {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    result,
                    commandName,
                    commandProperty,
                    commandId,
                    command);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}