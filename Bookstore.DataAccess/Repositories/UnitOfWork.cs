using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;

namespace Bookstore.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ApplicationDbContext _db;

        public ICategoryRepository CategoryRepository { get => _categoryRepository; }
        public IProductRepository ProductRepository { get => _productRepository; }
        public ICompanyRepository CompanyRepository { get => _companyRepository; }
        public IShoppingCartRepository ShoppingCartRepository { get => _shoppingCartRepository; }
        public IApplicationUserRepository ApplicationUserRepository { get => _applicationUserRepository; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            _categoryRepository = new CategoryRepository(_db);
            _productRepository = new ProductRepository(_db);
            _companyRepository = new CompanyRepository(_db);
            _shoppingCartRepository = new ShoppingCartRepository(_db);
            _applicationUserRepository = new ApplicationUserRepository(_db);
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
