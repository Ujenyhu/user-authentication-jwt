
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.DataAccess.Repository;
using userauthjwt.Models;

namespace userauthjwt.DataAccess.Repositories
{
    public class RepositoryWrapper(AppDbContext context, IConfiguration config, ICacheService cacheService) : IRepositoryWrapper
    {
       private AppDbContext _repoContext = context;
       private IConfiguration _config = config;
       private ICacheService _cacheService = cacheService;

       private  IUserRegRepository _UserRegRepository;
       private IUserRepository _UserRepository;
       private ISysConfigRepository _SysConfigRepository;
       private ILookupRepository _LookupRepository;

        public IUserRegRepository UserRegRepository
        {
            get
            {
                if(_UserRegRepository == null)
                {
                    _UserRegRepository = new UserRegRepository(_repoContext);
                }
                return _UserRegRepository;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (_UserRepository == null)
                {
                    _UserRepository = new UserRepository(_repoContext);
                }
                return _UserRepository;
            }
        }

        public ISysConfigRepository SysConfigRepository
        {
            get
            {
                if (_SysConfigRepository == null)
                {
                    _SysConfigRepository = new SysConfigRepository(_repoContext);
                }
                return _SysConfigRepository;
            }
        }

        public ILookupRepository LookupRepository
        {
            get
            {
                if (_LookupRepository == null)
                {
                    _LookupRepository = new LookupRepository(_repoContext, _cacheService);
                }
                return _LookupRepository;
            }
        }
    }
}
