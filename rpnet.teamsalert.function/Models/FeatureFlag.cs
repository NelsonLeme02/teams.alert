using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace rpnet.teamsalert.function.Models;

public class FeatureFlag
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MongoId { get; set; }
    public string? Name { get; set; }
    public bool Enabled { get; set; }

    public FeatureFlag()
    {

    }
}

