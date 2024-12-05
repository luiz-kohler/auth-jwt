using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infra
{
    public interface IUserRepository : IRepository<User>
    {
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(Context context) : base(context)
        {
        }
    }
}
