using userauthjwt.Models;
using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.DataAccess.Interfaces
{
    public interface ILookupRepository : IGenericRepository<MetaDataRef>
    {
    }
}
