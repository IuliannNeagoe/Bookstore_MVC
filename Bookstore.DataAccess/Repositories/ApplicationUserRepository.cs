using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;

namespace Bookstore.DataAccess.Repositories
{
    public class ApplicationUserRepository : RepositoryBase<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}  
