namespace API.Infra
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<TEntity> DeleteAsync(TEntity entity);
        Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate);
        Task<TEntity> GetAllAsync(Func<TEntity, bool>? predicate = null);
        Task<TEntity> SaveChangesAsync();
    }
}
