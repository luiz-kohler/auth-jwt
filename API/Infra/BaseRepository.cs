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
        Task<TEntity?> FirstOrDefaultWithNoTrackingAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool>? predicate = null);
        Task<IEnumerable<TEntity>> GetAllWithNoTrackingAsync(Func<TEntity, bool>? predicate = null);
        void Update(TEntity entity);
        Task SaveChangesAsync();
    }

    public class BaseRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected readonly Context _context;

        public BaseRepository(Context context)
        {
            _context = context;
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }


        public virtual async Task<TEntity?> FirstOrDefaultWithNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Func<TEntity, bool>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (predicate != null)
                query = query.Where(predicate).AsQueryable();

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllWithNoTrackingAsync(Func<TEntity, bool>? predicate = null)
        {
            var query = _context.Set<TEntity>().AsNoTracking().AsQueryable();

            if (predicate != null)
                query = query.Where(predicate).AsQueryable();

            return await query.ToListAsync();
        }

        public virtual void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
