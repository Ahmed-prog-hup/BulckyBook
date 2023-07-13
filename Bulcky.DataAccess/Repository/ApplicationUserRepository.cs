using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _dp;
        public ApplicationUserRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
  
    }
}
