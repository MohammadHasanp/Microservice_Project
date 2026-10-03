using Catalog.Api.Common;
using Catalog.Api.Context;
using Catalog.Api.Entities.Catalogs;

namespace Catalog.Api.Repository;

public interface IProductRepository : IBaseRepository<Product>
{
}

public class ProductRepository(CatalogContext context) : BaseRepository<Product>(context), IProductRepository;