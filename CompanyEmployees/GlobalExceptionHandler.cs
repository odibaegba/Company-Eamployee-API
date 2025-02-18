using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Contracts;
using Entities.ErrorModel;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CompanyEmployees
{
    public  class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILoggerManager _logger;
        public GlobalExceptionHandler(ILoggerManager logger)
        {
        _logger = logger;
        }
       public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
        {
                httpContext.Response.ContentType = "application/json";
                var contextFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    httpContext.Response.StatusCode = contextFeature.Error switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        BadRequestException => StatusCodes.Status400BadRequest,
                    _  => StatusCodes.Status500InternalServerError
                    };
                    _logger.LogError($"Something went wrong: {exception.Message}");
                    await httpContext.Response.WriteAsync(new ErrorDetails()
                    {
                    StatusCode = httpContext.Response.StatusCode,
                    ErrorMessage = contextFeature.Error.Message,
                    }.ToString());
                }
                return true;
        }
}   }