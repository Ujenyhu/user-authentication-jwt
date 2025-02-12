namespace userauthjwt.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
