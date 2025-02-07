using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rpnet.teamsalert.function.Handlers.Validators;

public class PathValidator : RequestHandler
{
    public override async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        if (context.Request.Path != "/alert")
            return ("Invalid path. Only '/alert' is allowed.", HttpStatusCode.NotFound);

        return await base.Handle(context);
    }
}
