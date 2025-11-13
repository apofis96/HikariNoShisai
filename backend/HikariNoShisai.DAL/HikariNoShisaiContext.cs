using HikariNoShisai.Common.Entities;
using HikariNoShisai.DAL.CompiledModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HikariNoShisai.DAL
{
    public class HikariNoShisaiContext(DbContextOptions<HikariNoShisaiContext> options) : DbContext(options)
    {
        public DbSet<Agent> Agents => Set<Agent>();

        public static readonly IModel CompiledModel = HikariNoShisaiContextModel.Instance;

        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseModel(CompiledModel);
        }*/
    }
}
