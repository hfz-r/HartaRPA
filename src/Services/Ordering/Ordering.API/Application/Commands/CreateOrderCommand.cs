using FluentValidation;
using Harta.Services.Ordering.API.Application.Validations;
using Harta.Services.Ordering.Grpc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harta.Services.Ordering.API.Application.Commands
{
    public class CreateOrderCommand : IRequest<bool>
    {
        public CreateOrderRequest Request { get; set; }
    }

    public class CommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CommandValidator(ILogger<CommandValidator> logger)
        {
            RuleFor(x => x.Request).SetValidator(new CreateOrderCommandValidator());

            logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }
    }
}