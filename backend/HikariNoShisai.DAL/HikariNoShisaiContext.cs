using HikariNoShisai.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var utcConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
                toDb => toDb.ToUniversalTime(),
                fromDb => fromDb.ToUniversalTime()
            );
            var combinedConverter = utcConverter.ComposeWith(new DateTimeOffsetToBinaryConverter());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset)
                             || p.PropertyType == typeof(DateTimeOffset?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(combinedConverter);
                }
            }
        }
    }
}
