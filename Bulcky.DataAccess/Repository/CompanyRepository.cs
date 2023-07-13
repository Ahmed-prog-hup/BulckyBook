using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _dp;
        public CompanyRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(Company obj)
        {
            _dp.Companies.Update(obj);
        }
    }
}
