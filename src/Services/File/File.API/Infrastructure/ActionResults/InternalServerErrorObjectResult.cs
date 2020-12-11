using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harta.Services.File.API.Infrastructure.ActionResults
{
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}