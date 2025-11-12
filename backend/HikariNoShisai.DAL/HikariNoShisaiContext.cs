using HikariNoShisai.Common.Entities;
using HikariNoShisai.DAL.CompiledModels;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.DAL
{
    public class HikariNoShisaiContext(DbContextOptions<HikariNoShisaiContext> options) : DbContext(options)
    {
        public DbSet<Agent> Agents => Set<Agent>();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseModel(HikariNoShisaiContextModel.Instance);
        }
    }
}
