using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace rpnet.teamsalert.function.Models;

public class LogException
{
    public LogException(Exception exception, string step)
    {
        Message = exception.Message;
        StackTrace = exception.StackTrace ?? "";
        Step = step;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MongoId { get; set; }
    public string? ApplicationName { get; set; } = "Teams Alert Function";
    public string ExceptionId { get; set; } = Ulid.NewUlid().ToString();
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string Step { get; set; }
    public DateTime OccuranceDate { get; set; } = DateTime.UtcNow.AddHours(-3);
}

