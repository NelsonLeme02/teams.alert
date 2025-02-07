using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rpnet.teamsalert.function.Handlers;

public interface IRequestHandler
{
    Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context);
    IRequestHandler SetNext(IRequestHandler next);
}
