using Catalog.Api.Context;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Catalog.Api.Common;

public interface IBaseRepository<TEntity> where TEntity : Entity
{
    public Task InsertAsync(TEntity entity);
    public Task<bool> UpdateAsync(TEntity entity);
    public Task<bool> DeleteAsync(string id);
    public Task<TEntity?> FindByIdAsync(string id);
    public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>>? where = null);
    public Task<IEnumerable<TEntity>> GetAllAsync();
}

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : Entity
{
    private readonly IMongoCollection<TEntity> _collection;
    protected BaseRepository(CatalogContext context)
    {
        var database = context.GetDataBase();
        _collection = database.GetCollection<TEntity>(typeof(TEntity).Name);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(d => d.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>>? where = null)
    {
        if (where != null)
        {
            var result = await _collection.FindAsync(where);
            return result.ToList();
        }

        var all = await _collection.FindAsync(_ => true);
        return all.ToList();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var products = await _collection.FindAsync(_ => true);
        return products.ToList();
    }

    public async Task<TEntity?> FindByIdAsync(string id)
    {
        var result = await _collection.FindAsync(f => f.Id == id);
        return result.FirstOrDefault();
    }

    public async Task InsertAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        var result = await _collection.ReplaceOneAsync(s => s.Id == entity.Id, entity);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}