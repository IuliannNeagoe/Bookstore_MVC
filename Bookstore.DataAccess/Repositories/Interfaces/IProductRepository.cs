using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public interface IProductRepository: IRepository<Product>
    {
        void Update(Product product);
    }
}
