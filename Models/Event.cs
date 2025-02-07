using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace rpnet.teamsalert.function.Models;

public class Event
{
    public Event(string eventName, HttpResponseMessage response)
    {
        EventName = eventName;
        Curl = SaveCurl(response).Result;
    }

    private static async Task<string> SaveCurl(HttpResponseMessage response)
    {
        if (response.RequestMessage == null)
            throw new InvalidOperationException("Response does not contain a request message.");

        var request = response.RequestMessage;
        var curlCommand = new StringBuilder("curl");

        curlCommand.Append($" -X {request.Method}");

        curlCommand.Append($" \"{request.RequestUri}\"");

        var headers = request.Headers
            .SelectMany(header => header.Value.Select(value => $"-H \"{header.Key}: {value}\""));

        var contentHeaders = request.Content?.Headers?
            .SelectMany(header => header.Value.Select(value => $"-H \"{header.Key}: {value}\""))
            ?? Enumerable.Empty<string>();

        curlCommand.Append($" {string.Join(" ", headers.Concat(contentHeaders))}");

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                curlCommand.Append($" -d \"{content.Replace("\"", "\\\"")}\"");
            }
        }

        return curlCommand.ToString();
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MongoId { get; set; }
    public string? ApplicationName { get; set; } = "Teams Alert Function";
    public string EventName { get; set; }
    public string Curl { get; set; }
    public string EventId { get; set; } = Ulid.NewUlid().ToString();
    public DateTime OccuranceDate { get; set; } = DateTime.UtcNow.AddHours(-3);
}

