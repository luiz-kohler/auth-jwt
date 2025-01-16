using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infra
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public Context(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(user => user.RefreshToken)
                .WithOne(refreshToken => refreshToken.User)
                .HasForeignKey<User>(user => user.RefreshTokenId)
                .IsRequired(false);
        }
    }
}
