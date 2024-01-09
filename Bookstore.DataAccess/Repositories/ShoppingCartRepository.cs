using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories
{
    public class ShoppingCartRepository : RepositoryBase<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart shoppingCart) => _db.ShoppingCarts.Update(shoppingCart);
    }
}
