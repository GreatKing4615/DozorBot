using Microsoft.EntityFrameworkCore;

namespace DozorBot.DAL.Contracts;

public interface IUnitOfWork<out TContext> : IUnitOfWork where TContext : DbContext
{
    TContext DbContext { get; }

    Task<int> SaveChangesAsync(CancellationToken token = default);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken token = default);
}