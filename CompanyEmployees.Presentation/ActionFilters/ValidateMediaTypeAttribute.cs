using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace CompanyEmployees.Presentation.ActionFilters
{
    public class ValidateMediaTypeAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var acceptHeaderPresent = context.HttpContext
            .Request.Headers.ContainsKey("Accept");

            if(!acceptHeaderPresent)
            {
                context.Result = new BadRequestObjectResult($"Accept header is missing.");
            }

            var mediaType = context.HttpContext
            .Request.Headers["Accept"].FirstOrDefault();

            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue? outMediaType))
            {
                context.Result = new BadRequestObjectResult($"Media type not peresent. Please add Accept header with the required media type.");
                return;
            }

            context.HttpContext.Items.Add("AcceptHeaderMediaType", outMediaType);
        }
    }
}