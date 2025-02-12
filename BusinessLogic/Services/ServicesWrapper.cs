
using Microsoft.Extensions.Caching.Memory;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.BusinessLogic.Services.User;
using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.BusinessLogic.Services
{
    public class ServicesWrapper : IServicesWrapper
    {
        private IConfiguration _config;
        private IRepositoryWrapper _repository;
        private IUnitOfWork _unitOfWork;
        private IHttpContextAccessor _HttpContext;
        private IWebHostEnvironment _WebHostEnvironment;
        private HttpClient _httpClient;



        //services
        private ILookupService _LookupService;
        private IAuthenticationService _AuthenticationService;
        private IUserService _UserService;
        private IUserRegistrationService _UserRegistrationService;
        private IMailService _MailService;
        private ISmsService _SmsService;

        public ServicesWrapper(IRepositoryWrapper repository,
           IConfiguration config,
           IUnitOfWork unitOfWork,
           IMemoryCache cache,
           IWebHostEnvironment webHostEnvironment,
           IHttpContextAccessor httpContext,
           HttpClient httpClient)
        {
            _repository = repository;
            _config = config;
            _HttpContext = httpContext;
            _unitOfWork = unitOfWork;
            _WebHostEnvironment = webHostEnvironment;
            _httpClient = httpClient;
        }

        public IAuthenticationService AuthenticationService
        {
            get
            {
                if (_AuthenticationService == null)
                {
                    _AuthenticationService = new AuthenticationService(_config, _HttpContext, _repository);
                }
                return _AuthenticationService;
            }
        }

        public IMailService MailService
        {
            get
            {
                if (_MailService == null)
                {
                    _MailService = new MailService(_repository, AuthenticationService, _WebHostEnvironment);
                }
                return _MailService;
            }
        }

        public ISmsService SmsService
        {
            get
            {
                if (_SmsService == null)
                {
                    _SmsService = new SmsService(_repository);
                }
                return _SmsService;
            }
        }

        public IUserRegistrationService UserRegistrationService
        {
            get
            {
                if (_UserRegistrationService == null)
                {
                    _UserRegistrationService = new UserRegistrationService(_repository, AuthenticationService, _unitOfWork, MailService, SmsService, _config, _WebHostEnvironment, _HttpContext);
                }
                return _UserRegistrationService;
            }
        }

        public IUserService UserService
        {
            get
            {
                if (_UserService == null)
                {
                    _UserService = new UserService(_repository, AuthenticationService, _unitOfWork, MailService, SmsService, _config, _WebHostEnvironment, _HttpContext);
                }
                return _UserService;
            }
        }

        public ILookupService LookupService
        {
            get
            {
                if (_LookupService == null)
                {
                    _LookupService = new LookupService(_repository, _unitOfWork);
                }
                return _LookupService;
            }
        }
    }
}
