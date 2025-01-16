using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Infra
{
    public interface IUserRepository : IRepository<User>
    {
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(Context context) : base(context)
        {
        }
    }

    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
    }

    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(Context context) : base(context)
        {
        }

        public override async Task<RefreshToken?> FirstOrDefaultAsync(Expression<Func<RefreshToken, bool>> predicate)
        {
            return await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(predicate);

        }
    }
}
