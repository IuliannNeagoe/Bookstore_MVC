using Bookstore.DataAccess.Data;
using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public class OrderHeaderRepository : RepositoryBase<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader) => _db.OrderHeaders.Update(orderHeader);
    }
}
