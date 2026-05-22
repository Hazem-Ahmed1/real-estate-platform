
namespace DataAccessLayer.Contracts;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = false);
    Task<TEntity?> GetByIdAsync(int id);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    #region Specifications
    Task<IEnumerable<TEntity>> GetAllAsync(ISpecification<TEntity> specification);
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetByIdAsync(ISpecification<TEntity> specification);
    Task<int> CountAsync(ISpecification<TEntity> specification);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    #endregion
}
