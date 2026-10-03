using Catalog.Api.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Api.Entities.Catalogs;

public class Product : Entity
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Summery { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImageFile { get; set; } = null!;
    public decimal Price { get; set; }
}