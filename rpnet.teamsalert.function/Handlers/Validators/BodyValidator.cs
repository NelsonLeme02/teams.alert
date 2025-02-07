using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rpnet.teamsalert.function.Handlers.Validators;

public class BodyValidator : RequestHandler
{
    public override async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        string body;
        using (var reader = new StreamReader(context.Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        if (body == null || body.Length == 0)
            return ("Request body cannot be empty.", HttpStatusCode.BadRequest);

        if (string.IsNullOrWhiteSpace(body))
            return ("Request body must contain valid content.", HttpStatusCode.BadRequest);

        context.Items.Add("RequestBody", body);
        return await base.Handle(context);
    }
}
