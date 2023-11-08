using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using userauthjwt.Interfaces;
using userauthjwt.Models;
using userauthjwt.Responses;

namespace userauthjwt.Repository
{
   #pragma warning disable CS8603

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private AppDbContext _context;
        private DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            
        }
        public async void AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void UpdateAsync(T obj)
        {
            _dbSet.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }
        public async void DeleteAsync(object id)
        {
            var ifExist = await _dbSet.FindAsync(id);
            if (ifExist != null)
            {
                _dbSet.Remove(ifExist);
            }
        }

        public async Task<ResponseBase<T>> GetByIdAsync(string id)
        {
            var response = await _dbSet.FindAsync(id);
            return new ResponseBase<T>(response);
        }
        public async Task<T> FindByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> GetFirstOrDefaultAsync()
        {
            return await _context.Set<T>().FirstOrDefaultAsync();
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
