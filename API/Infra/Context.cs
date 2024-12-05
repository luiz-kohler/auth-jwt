using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infra
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; }
        public Context(DbContextOptions options) : base(options) { }
    }
}
