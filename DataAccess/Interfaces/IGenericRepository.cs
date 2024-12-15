using System.Linq.Expressions;
using userauthjwt.Responses;

namespace userauthjwt.DataAccess.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<bool> GetAnyAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync();
        Task<T> GetByIdAsync(long id);
        Task<List<T>> GetBySQLRaw(string statement);
        Task<T> FindByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRange(List<T> entities);
        Task Update(T obj);
        Task DeleteAsync(object id);
    }
}
