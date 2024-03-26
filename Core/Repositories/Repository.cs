using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace RESTful_web_API_Course.Core; 

public abstract class Repository<T>(DbContext context) : IRepository<T> where T : class {
    
    public IQueryable<T> ReadAll(bool trackChanges = true) 
        => false == trackChanges
            ? context.Set<T>().AsNoTracking()
            : context.Set<T>();

    public IQueryable<T> ReadByCondition(Expression<Func<T, bool>> expression, bool trackChanges = true) 
        => false == trackChanges 
            ? context.Set<T>().Where(expression).AsNoTracking() 
            : context.Set<T>().Where(expression);
    
    public void Create(T entity) 
        => context.Set<T>().Add(entity);

    public void Update(T entity) 
        => context.Set<T>().Update(entity);

    public void Delete(T entity) 
        => context.Set<T>().Remove(entity);

    public Task SaveAsync()
        => context.SaveChangesAsync();
}