using Bulcky.Models;

namespace Bulcky.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHaeder>
    {
        void Update(OrderHaeder obj);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripPaymentId(int id,string sessionId,string paymentintentId);
        
    }
}
