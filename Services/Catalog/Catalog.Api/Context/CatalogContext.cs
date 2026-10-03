using Catalog.Api.Common.Models;
using MongoDB.Driver;

namespace Catalog.Api.Context;

public class CatalogContext(IMongoClient client, MongoSettings settings)
{
    private readonly IMongoDatabase _database = client.GetDatabase(settings.DataBaseName);
    public IMongoDatabase GetDataBase()
    {
        return _database;
    }
    
}