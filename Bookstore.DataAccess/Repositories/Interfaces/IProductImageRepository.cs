using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}
