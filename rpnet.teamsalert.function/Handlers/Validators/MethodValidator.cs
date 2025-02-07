using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rpnet.teamsalert.function.Handlers.Validators;

public class MethodValidator : RequestHandler
{
    public override async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Post)
            return ($"Method '{context.Request.Method}' is not allowed. Use POST instead.", HttpStatusCode.MethodNotAllowed);

        return await base.Handle(context);
    }
}
