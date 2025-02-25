namespace userauthjwt.BusinessLogic.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        void SetAsync<T>(string key, T value, TimeSpan? expiration);
        void RemoveAsync(string key);
    }
}
