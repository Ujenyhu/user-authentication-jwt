
using userauthjwt.Responses;
using userauthjwt.Responses.Lookup;

namespace userauthjwt.BusinessLogic.Interfaces
{
    public interface ILookupService
    {
        Task<ResponseBase<List<LookupResponse>>> OtpRequestTypes();
        Task<ResponseBase<List<LookupResponse>>> GetUserStatus();
        Task<ResponseBase<List<LookupResponse>>> GetUserTypes();

    }
}
