using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using rpnet.teamsalert.function.Models;

namespace rpnet.teamsalert.function.Core.Persistence;

public class MongoDbContext
{
    private readonly MongoClient mongoClient;
    private readonly string connectionString;
    private readonly Dictionary<Type, (string Database, string Collection)> collections;

    public MongoDbContext(string connectionString)
    {
        this.connectionString = connectionString;
        mongoClient = new MongoClient(this.connectionString);
        collections = new Dictionary<Type, (string Database, string Collection)>
            {
                { typeof(Event), ("logging","Event")},
                { typeof(LogException), ("logging","Exception")},
                { typeof(FeatureFlag), ("setup","FeatureFlag")}
            };
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        if (!collections.TryGetValue(typeof(T), out var collection))
            throw new InvalidOperationException($"Collection{typeof(T).Name} não encontrada");
        return mongoClient.GetDatabase(collection.Database).GetCollection<T>(collection.Collection);
    }
    
    public async Task Insert<T>(IMongoCollection<T> collection, T data)
    {
        try
        {
            await collection.InsertOneAsync(data);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

