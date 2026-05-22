
namespace DataAccessLayer.Contracts;

public interface ISpecification<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>? Criteria { get; }
    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; }
    public List<string> IncludeStrings { get; }
    public Expression<Func<TEntity, object>> OrderBy { get; }
    public Expression<Func<TEntity, object>> OrderByDescending { get; }
    public int Take { get; }
    public int Skip { get; }
    public bool IsPaginated { get; }
}
