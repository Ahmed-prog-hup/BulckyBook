using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;

namespace Bulcky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHaeder>, IOrderHeaderRepository
    {
        private ApplicationDbContext _dp;
        public OrderHeaderRepository(ApplicationDbContext dp) :base(dp)
        {
            _dp = dp;
        }
      

        public void Update(OrderHaeder obj)
        {
            _dp.OrderHaeders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderFromDb =_dp.OrderHaeders.FirstOrDefault(x => x.Id == id);
            if(orderFromDb  != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
		}

		public void UpdateStripPaymentId(int id, string sessionId, string paymentintentId)
		{
			var orderFromDb = _dp.OrderHaeders.FirstOrDefault(x => x.Id == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentintentId))
            {
                orderFromDb.PaymentIntentId = paymentintentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
		}
	}
}
