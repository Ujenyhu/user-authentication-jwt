using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Models;
using userauthjwt.Responses;

namespace userauthjwt.DataAccess.Repository
{
#pragma warning disable CS8603

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private AppDbContext _context;
        private DbSet<T> _entities;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = context.Set<T>();

        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await _context.Set<T>().FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetBySQLRaw(string statement)
        {
            return await _context.Set<T>().FromSqlRaw(statement).ToListAsync();
        }

        public async Task<bool> GetAnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }

        public async Task<T> GetByIdAsync(long id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task<T> FindByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(predicate);
        }

        public async Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            return query;
        }

        public async Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate)
        {
            IEnumerable<T> query = (IEnumerable<T>)_context.Set<T>().Where(predicate);
            return query;
        }

        public async Task AddAsync(T entity)
        {
            await _entities.AddAsync(entity);
        }

        public async Task AddRange(List<T> entities)
        {
            await _entities.AddRangeAsync(entities);
        }

        public async Task Update(T obj)
        {
            _entities.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }

        public async Task DeleteAsync(object id)
        {
            T existing = await _entities.FindAsync(id);
            _entities.Remove(existing);
        }

    }
}
