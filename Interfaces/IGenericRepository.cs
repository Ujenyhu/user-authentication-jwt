using System.Linq.Expressions;
using userauthjwt.Responses;

namespace userauthjwt.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        //Task<List<T>> GetAllByConditionAsync(Expression<Func<T, bool>> condition);
        Task<T> FindByConditionAsync(Expression<Func<T, bool>> condition);
        Task<ResponseBase<T>> GetByIdAsync(string id);
        Task<T> GetFirstOrDefaultAsync();
        void AddAsync(T entity);
        void UpdateAsync(T obj); 
        void DeleteAsync(object id);
        Task CompleteAsync();
    }
}
