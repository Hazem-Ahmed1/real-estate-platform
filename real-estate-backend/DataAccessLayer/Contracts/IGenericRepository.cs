using DataAccessLayer.Entities;
using System.Linq.Expressions;

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
    Task<TEntity?> GetByIdAsync(ISpecification<TEntity> specification);
    Task<int> CountAsync(ISpecification<TEntity> specification);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    #endregion
}
