using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;

namespace Bookstore.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _db;
        public ICategoryRepository CategoryRepository { get => _categoryRepository; }
        public IProductRepository ProductRepository { get => _productRepository; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            _categoryRepository = new CategoryRepository(_db);
            _productRepository = new ProductRepository(_db);
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
