using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using rpnet.teamsalert.function.Core.Persistence;
using rpnet.teamsalert.function.Models;

namespace rpnet.teamsalert.function.StaticServices;

public static class StaticRepository
{
    private static string? connectionString;
    private static string GetConnectionString()
    {
        DotNetEnv.Env.Load();
        return Environment.GetEnvironmentVariable("MONGO_DB_CONNECT") ?? "";
    }

    private static MongoDbContext? mongoContext;

    public static async Task<FeatureFlag?> GetFeatureFlagByName(string name)
    {
        connectionString = GetConnectionString();
        mongoContext = new(connectionString);
        var collection = mongoContext.GetCollection<FeatureFlag>();
        var filter = Builders<FeatureFlag>.Filter.Eq(x => x.Name, name);
        var cursor = await collection.Find(filter).Limit(1).ToListAsync();
        return cursor.FirstOrDefault();
    }

    public static async Task Log(LogException data)
    {
        if (connectionString is null)
        {
            connectionString = GetConnectionString();
            mongoContext = new(connectionString);
        }
        var collection = mongoContext.GetCollection<LogException>();
        await mongoContext.Insert(collection, data);
    }

    public static async Task Event(Event data)
    {
        if (connectionString is null)
        {
            connectionString = GetConnectionString();
            mongoContext = new(connectionString);
        }
        var collection = mongoContext.GetCollection<Event>();
        await mongoContext.Insert(collection, data);
    }
}