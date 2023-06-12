namespace DozorBot.DAL.Contracts;
public interface IRepositoryFactory
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
}