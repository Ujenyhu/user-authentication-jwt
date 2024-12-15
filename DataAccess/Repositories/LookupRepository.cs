
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Models;
using userauthjwt.DataAccess.Repository;

namespace userauthjwt.DataAccess.Repositories
{
    public class LookupRepository : GenericRepository<MetaDataRef>, ILookupRepository
    {
        private readonly AppDbContext _context;

        public LookupRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
