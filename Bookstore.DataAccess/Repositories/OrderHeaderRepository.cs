using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Utility;

namespace Bookstore.DataAccess.Repositories
{
    public class OrderHeaderRepository : RepositoryBase<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader) => _db.OrderHeaders.Update(orderHeader);
        public void UpdateStatus(int id, OrderStatus orderStatus, PaymentStatus? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (orderFromDb == null) return;

            orderFromDb.OrderStatus = orderStatus.ToString();
            if (paymentStatus != null)
            {
                orderFromDb.PaymentStatus = paymentStatus.ToString();
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (orderFromDb == null) return;

            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }

            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
