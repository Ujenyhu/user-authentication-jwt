using userauthjwt.Models.User;
using userauthjwt.Requests;
using userauthjwt.Responses;

namespace userauthjwt.DataAccess.Interfaces
{
    public interface IUserRepository : IGenericRepository<UserProfile>
    {
        Task AddOtpRecordAsync(OtpRecord request);
        Task UpdateOtpRecord(OtpRecord model);
        Task<OtpRecord> GetNewestOtpRecord(string UserId);
        Task AddFailedSecurityAttemptsAsync(FailedSecurityAttempt request);
    }
}
