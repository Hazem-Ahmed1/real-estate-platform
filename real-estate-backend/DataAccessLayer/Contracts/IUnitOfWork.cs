
namespace DataAccessLayer.Contracts;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> CompleteAsync();
}
