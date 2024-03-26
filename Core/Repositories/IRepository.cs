using System.Linq.Expressions;

namespace RESTful_web_API_Course.Core; 

public interface IRepository<T> {
    IQueryable<T> ReadAll(bool trackChanges);
    IQueryable<T> ReadByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveAsync();
}