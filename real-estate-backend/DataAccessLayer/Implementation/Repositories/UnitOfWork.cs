using System.Collections;
using DataAccessLayer.Contracts;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;

namespace DataAccessLayer.Implementation.Repositories;

public class UnitOfWork(RealEstateDbContext context) : IUnitOfWork
{
    private Hashtable? _repositories;

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        _repositories ??= new Hashtable();

        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), context);

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type]!;
    }

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
