using Microsoft.EntityFrameworkCore;

namespace LineNotifier.Models.Db
{
    public class LineNotifierDbContext : DbContext
    {
        public LineNotifierDbContext(DbContextOptions options): base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.UserId)
                    .HasPrincipalKey(p => p.LineUserId);
            });
        }
    }
}
