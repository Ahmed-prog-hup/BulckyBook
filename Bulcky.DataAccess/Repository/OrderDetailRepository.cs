using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _dp;
        public OrderDetailRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(OrderDetail obj)
        {
            _dp.OrderDetails.Update(obj);
        }
    }
}
