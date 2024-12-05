using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Infra
{
    public interface IRepository<TEntity>
            where TEntity : class
    {
        Task AddAsync(TEntity entity);
        void Remove(TEntity entity);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FirstOrDefaultWithWithNoTrackingAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool>? predicate = null);
        Task<IEnumerable<TEntity>> GetAllWithNoTrackingAsync(Func<TEntity, bool>? predicate = null);
        void Update(TEntity entity);
        Task SaveChangesAsync();
    }

    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }


        public async Task<TEntity?> FirstOrDefaultWithWithNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (predicate != null)
                query = query.Where(predicate).AsQueryable();

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllWithNoTrackingAsync(Func<TEntity, bool>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsNoTracking().AsQueryable();

            if (predicate != null)
                query = query.Where(predicate).AsQueryable();

            return await query.ToListAsync();
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
