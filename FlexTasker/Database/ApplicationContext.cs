using Microsoft.EntityFrameworkCore;

namespace FlexTasker.Database
{
	public class ApplicationContext : DbContext
	{
		public DbSet<Models.User> users { get; set; } = null!;
		public DbSet<Models.TodoList> todoLists { get; set; } = null!;
		public DbSet<Models.TodoItem> todoItems { get; set; } = null!;
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=UsersDB;Username=postgres;Password=12345");
		}
	}
}
