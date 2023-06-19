using DozorBot.DAL.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DozorBot.DAL.UnitOfWork;

public sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{

    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();
    }

    public async Task<TResult?> SingleOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        CancellationToken token = default,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? await orderBy(query).Select(selector).FirstOrDefaultAsync(token)
            : await query.Select(selector).FirstOrDefaultAsync(token);
    }

    public IQueryable<TResult?> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        bool ignoreAutoIncludes = false) where TResult : class
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (ignoreAutoIncludes)
        {
            query = query.IgnoreAutoIncludes();
        }

        return orderBy is not null
            ? orderBy(query).Select(selector)
            : query.Select(selector);
    }

    public Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        _dbSet.AddRangeAsync(entities, cancellationToken);

    public ValueTask<EntityEntry<TEntity>> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        _dbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Delete(TEntity entity) => _dbSet.Remove(entity);
    public void DeleteRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);
}