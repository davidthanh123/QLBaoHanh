using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QLBaoHanh_UNETI04_DHTI17A4HN.Data;

namespace QLBaoHanh_UNETI04_DHTI17A4HN.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

        public virtual IQueryable<T> AsNoTracking() => _dbSet.AsNoTracking();

        public virtual Task<List<T>> GetAllAsync() => _dbSet.ToListAsync();

        public virtual async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.FirstOrDefaultAsync(predicate);

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            _dbSet.AnyAsync(predicate);

        public virtual Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? _dbSet.CountAsync() : _dbSet.CountAsync(predicate);

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Remove(T entity) => _dbSet.Remove(entity);

        public virtual void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        public virtual Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
