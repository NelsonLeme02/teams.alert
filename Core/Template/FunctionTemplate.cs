using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using rpnet.teamsalert.function.Handlers.Validators;
using rpnet.teamsalert.function.Models;
using rpnet.teamsalert.function.StaticServices;

namespace rpnet.teamsalert.function.Core.Template;

public class FunctionTemplate
{
    public FunctionTemplate()
    {

    }

    protected static async Task<bool> IsFeatureFlagEnabled(string feature)
    {
        var flag = await StaticRepository.GetFeatureFlagByName(feature);

        if (flag is null)
            return false;

        return flag.Enabled;
    }

    protected async Task<(string message, HttpStatusCode statusCode)> Send(HttpContext context)
    {
        var handlerChain = new PathValidator();
        handlerChain
            .SetNext(new MethodValidator())
            .SetNext(new BodyValidator())
            .SetNext(new NeedsAlertValidator())
            .SetNext(new AlertHandler());

        return await handlerChain.Handle(context);
    }
}
