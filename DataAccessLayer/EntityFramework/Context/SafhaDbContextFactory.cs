using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccessLayer.EntityFramework.Context
{
    public class SafhaDbContextFactory : IDesignTimeDbContextFactory<SafhaDbContext>
    {
        public SafhaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SafhaDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=safhaDb;Username=postgres;Password=1967");

            return new SafhaDbContext(optionsBuilder.Options);
        }
    }
}
