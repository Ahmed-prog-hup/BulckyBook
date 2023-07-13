using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _dp;
        public CategoryRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(Category obj)
        {
            _dp.Categories.Update(obj);
        }
    }
}
