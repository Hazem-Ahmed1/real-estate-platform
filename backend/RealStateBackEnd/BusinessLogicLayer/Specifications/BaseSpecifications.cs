using System.Linq.Expressions;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Specifications;

public abstract class BaseSpecifications<TEntity> : ISpecification<TEntity> 
    where TEntity : class
{
    protected BaseSpecifications() { }

    protected BaseSpecifications(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<TEntity, object>> OrderBy { get; private set; } = null!;
    public Expression<Func<TEntity, object>> OrderByDescending { get; private set; } = null!;
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPaginated { get; private set; }

    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        IncludeExpressions.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    protected void ApplyPagination(int pageSize, int pageIndex)
    {
        IsPaginated = true;
        Take = pageSize;
        Skip = pageSize * (pageIndex - 1);
    }
}
