using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bookstore.DataAccess.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private DbSet<T> _dbSet;
        public RepositoryBase(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>();  //_db.Categories == dbSet
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }
        public void Add(T entity) => _dbSet.Add(entity);

        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public void Remove(T entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    }
}
