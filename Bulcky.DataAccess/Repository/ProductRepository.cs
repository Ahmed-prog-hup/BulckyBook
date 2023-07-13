using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _dp;
        public ProductRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(Product obj)
        {
            _dp.Products.Update(obj);
        }
    }
}
