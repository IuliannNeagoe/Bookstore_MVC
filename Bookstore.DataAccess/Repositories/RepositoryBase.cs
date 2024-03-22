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

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            //AsNoTracking() => repository.Update(entity) is mandatory
            //By default, Update doesnt have to be called (if you modify an entityFromDb then Save, it will get updated
            IQueryable<T> query = tracked ? _dbSet : _dbSet.AsNoTracking();

            ExecuteObjectToForeignKeyRelation(ref query, includeProperties);

            query = query.Where(filter);
            return query.FirstOrDefault();
        }



        public void Add(T entity) => _dbSet.Add(entity);

        //includeProperties - Category    - this is used to map an actual CategoryObject with the existing CategoryId
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet.AsQueryable<T>();
            ExecuteObjectToForeignKeyRelation(ref query, includeProperties);
            return query.ToList();
        }

        public void Remove(T entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        private void ExecuteObjectToForeignKeyRelation(ref IQueryable<T> query, string? includeProperties)
        {
            if (!string.IsNullOrEmpty(includeProperties))
            {
                var propertiesToInclude = includeProperties.Split(",", StringSplitOptions.RemoveEmptyEntries);
                foreach (var property in propertiesToInclude)
                {
                    query = query.Include(property);
                }
            }
        }
    }
}
