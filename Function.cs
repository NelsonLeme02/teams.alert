using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using rpnet.teamsalert.function.Core.Template;
using rpnet.teamsalert.function.Models;
using rpnet.teamsalert.function.StaticServices;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rpnet.teamsalert.function;

public class Function : FunctionTemplate, IHttpFunction
{
    public async Task HandleAsync(HttpContext context)
    {
        try
        {
            if (!await IsFeatureFlagEnabled("TeamsAlertFunction"))
            {
                await context.Response.WriteAsync("Alert Function is not enabled");
                return;
            }

            var (message, statusCode) = await Send(context);
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(message);
        }
        catch (Exception ex)
        {
            var log = new LogException(ex, "Entry point at TeamsAlertFunction");
            await StaticRepository.Log(log);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync($"Error: {log.Message}, Code: {log.ExceptionId}");
        }
    }
}
