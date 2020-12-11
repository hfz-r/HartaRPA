using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Harta.Services.File.API.Infrastructure.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            var validationErrors = context.ModelState
                .Keys
                .SelectMany(x => context.ModelState[x].Errors)
                .Select(x => x.ErrorMessage)
                .ToArray();

            var json = new JsonErrorResponse {Messages = validationErrors};

            context.Result = new BadRequestObjectResult(json);
        }
    }
}