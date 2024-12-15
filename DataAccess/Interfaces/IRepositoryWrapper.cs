using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.DataAccess.Interfaces
{
    public interface IRepositoryWrapper
    {
        IUserRepository UserRepository { get; }
        IUserRegRepository UserRegRepository { get; }
        ISysConfigRepository SysConfigRepository { get; }
        ILookupRepository LookupRepository { get; }
    }
}
