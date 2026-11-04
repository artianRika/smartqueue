using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QueueApi.Infrastructure;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ConflictException conflict)
            return;

        context.Result = new ConflictObjectResult(conflict.Message);
        context.ExceptionHandled = true;
    }
}
