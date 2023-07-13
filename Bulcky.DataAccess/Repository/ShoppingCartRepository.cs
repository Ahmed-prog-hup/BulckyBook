using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _dp;
        public ShoppingCartRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(ShoppingCart obj)
        {
            _dp.ShoppingCarts.Update(obj);
        }
    }
}
