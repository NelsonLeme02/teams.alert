using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using rpnet.teamsalert.function.Models;

namespace rpnet.teamsalert.function.Handlers.Validators;

public class AlertHandler : RequestHandler
{
    public override async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        var objectData = (ExpectedRequest?)context.Items["DeserializedObject"] ?? new ExpectedRequest();
        if (await Alert(objectData))
            return ("Alert received successfully.", HttpStatusCode.OK);

        return ("Failed to send Alert.", HttpStatusCode.NotFound);
    }
}
