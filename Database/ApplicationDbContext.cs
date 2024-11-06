using Microsoft.EntityFrameworkCore;

namespace CISOServer.Database
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<DbUser> users { get; set; }
		private static string connectionString;

		public static void Init(string connectionString)
		{
			ApplicationDbContext.connectionString = connectionString;
		}

		public ApplicationDbContext()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql(connectionString);
		}
	}
}
