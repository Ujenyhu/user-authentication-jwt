
using Microsoft.EntityFrameworkCore;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.DataAccess.Repository;
using userauthjwt.Models;
using userauthjwt.Models.Configs;

namespace userauthjwt.DataAccess.Repositories
{
    public class SysConfigRepository : GenericRepository<SysConfig>, ISysConfigRepository
    {
        private readonly AppDbContext _context;

        public SysConfigRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SysConfig> GetSysConfig()
        {
            return await _context.SysConfig.FirstOrDefaultAsync();
        }

        public async Task<MailConfig> GetMailConfigFirstOrDefaultAsync()
        {
            return await _context.MailConfig.FirstOrDefaultAsync();
        }

        public async Task<SmsConfig> GetSmsConfigFirstOrDefaultAsync()
        {
            return await _context.SmsConfig.FirstOrDefaultAsync();
        }
    }
}
