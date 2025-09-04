using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepositoryPattern.Models;

namespace RepositoryPattern
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration? _configuration;

        public DatabaseContext()
        {
        }

        public DatabaseContext(
            IConfiguration configuration,
            DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            _configuration = configuration;
        }
	
	        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseNpgsql();
            }

            base.OnConfiguring(optionsBuilder);

            optionsBuilder.EnableSensitiveDataLogging();
        }
	
	    public DbSet<Test> TestCollection { get; set; } = null!;

    }
}
