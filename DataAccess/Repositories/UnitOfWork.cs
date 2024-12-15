using Microsoft.EntityFrameworkCore.Storage;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Models;

namespace userauthjwt.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }


        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
