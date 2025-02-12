
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.Models;
using System.Net;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using userauthjwt.Responses;
using userauthjwt.Responses.Lookup;


namespace userauthjwt.BusinessLogic.Services
{
    public class LookupService : ILookupService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IUnitOfWork _unitOfWork;


        public LookupService(IRepositoryWrapper repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }



        public async Task<ResponseBase<List<LookupResponse>>> OtpRequestTypes()
        {
            var filteredQuery = await _repository.LookupRepository.WhereAsync(m => m.MetaDataType == VarHelper.MetaDataTypes.OTP_TYPE.ToString());
            var refs = filteredQuery.ToList();

            var response = new List<LookupResponse>();

            Mapper<MetaDataRef, LookupResponse>.mapList(refs, response);

            return new ResponseBase<List<LookupResponse>>(response, 200, "Successfully retrieved", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<List<LookupResponse>>> GetUserStatus()
        {
            var filteredQuery = await _repository.LookupRepository.WhereAsync(m => m.MetaDataType == VarHelper.MetaDataTypes.USER_STATUS.ToString());
            var refs = filteredQuery.ToList();
            //var refs = filteredQuery.Select(x => x.ReferenceName.Replace("_", " ")).ToList();

            var response = new List<LookupResponse>();

            //Mapper<string , LookupResponse>.mapList(refs, response);
            Mapper<MetaDataRef, LookupResponse>.mapList(refs, response);

            return new ResponseBase<List<LookupResponse>>(response, 200, "Successfully retrieved", VarHelper.ResponseStatus.SUCCESS.ToString());
        }

        public async Task<ResponseBase<List<LookupResponse>>> GetUserTypes()
        {
            var filteredQuery = await _repository.LookupRepository.WhereAsync(m => m.MetaDataType == VarHelper.MetaDataTypes.USER_TYPE.ToString());
            var refs = filteredQuery.ToList();
            //var refs = filteredQuery.Select(x => x.ReferenceName.Replace("_", " ")).ToList();

            var response = new List<LookupResponse>();

            //Mapper<string , LookupResponse>.mapList(refs, response);
            Mapper<MetaDataRef, LookupResponse>.mapList(refs, response);

            return new ResponseBase<List<LookupResponse>>(response, 200, "Successfully retrieved", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


    }
}
