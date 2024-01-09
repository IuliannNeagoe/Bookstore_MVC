using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public interface IShoppingCartRepository: IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
    }
}
