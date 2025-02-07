using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using rpnet.teamsalert.function.Models;

namespace rpnet.teamsalert.function.Handlers.Validators;

public class NeedsAlertValidator : RequestHandler
{
    public override async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        var content = context.Items.FirstOrDefault(x => x.Key.Equals("RequestBody"));
        var objectData = Deserialize<ExpectedRequest>(content.Value.ToString());

        if (!NeedsAlert(ref objectData))
            return ("No need for alert", HttpStatusCode.Found);

        context.Items.Add("DeserializedObject", objectData);
        return await base.Handle(context);
    }
}
