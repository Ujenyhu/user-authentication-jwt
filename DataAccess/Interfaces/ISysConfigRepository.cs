
using userauthjwt.Models.Configs;

namespace userauthjwt.DataAccess.Interfaces
{
    public interface ISysConfigRepository : IGenericRepository<SysConfig>
    {
        Task<SysConfig> GetSysConfig();
        Task<MailConfig> GetMailConfigFirstOrDefaultAsync();
        Task<SmsConfig> GetSmsConfigFirstOrDefaultAsync();

    }
}
