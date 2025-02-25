
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Models;
using userauthjwt.DataAccess.Repository;
using userauthjwt.BusinessLogic.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace userauthjwt.DataAccess.Repositories
{
    public class LookupRepository : GenericRepository<MetaDataRef>, ILookupRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;
        private const string cacheTitleCountries = "Countries";

        public LookupRepository(AppDbContext context, ICacheService cacheService) : base(context)
        {
            _context = context;
            _cacheService = cacheService;
        }


        public async Task<List<Country>> GetCountries()
        {
            // Attempt to retrieve the cached result
            var data = await _cacheService.GetAsync<List<Country>>(cacheTitleCountries);

            if (data != null) return data;

            // If not found in cache, load from the db
            var countries = await _context.Country.Where(x => x.IsActive).ToListAsync();

            // Cache the result with a 1hr expiration
            _cacheService.SetAsync(cacheTitleCountries, countries, TimeSpan.FromMinutes(60));

            return countries;
        }
    }
}
