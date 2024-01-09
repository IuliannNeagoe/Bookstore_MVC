namespace Bookstore.DataAccess.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; }
        public IProductRepository ProductRepository { get; }
        public ICompanyRepository CompanyRepository { get; }
        public IShoppingCartRepository ShoppingCartRepository { get; }

        public void Save();
    }
}
