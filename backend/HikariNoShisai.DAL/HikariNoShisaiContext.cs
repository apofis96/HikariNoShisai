using HikariNoShisai.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.DAL
{
    public class HikariNoShisaiContext(DbContextOptions<HikariNoShisaiContext> options) : DbContext(options)
    {
        public DbSet<Agent> Agents => Set<Agent>();
        public DbSet<AgentTerminal> AgentTerminals => Set<AgentTerminal>();
        public DbSet<AgentStatusLog> AgentStatusLogs => Set<AgentStatusLog>();
        public DbSet<User> Users => Set<User>();

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is not BaseEntity entity)
                {
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTimeOffset.UtcNow;
                    entity.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
