using Bookstore.Models.Models;
using Bookstore.Utility;

namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, OrderStatus orderStatus, PaymentStatus? paymentStatus = null);
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
    }
}
