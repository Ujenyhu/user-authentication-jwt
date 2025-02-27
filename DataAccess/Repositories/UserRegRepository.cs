using userauthjwt.DataAccess.Interfaces;
using userauthjwt.DataAccess.Repository;
using userauthjwt.Models;
using userauthjwt.Models.User;

namespace userauthjwt.DataAccess.Repositories
{
    public class UserRegRepository : GenericRepository<UserRegistration>, IUserRegRepository
    {
        private readonly AppDbContext _context;
        public UserRegRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


    }
}

