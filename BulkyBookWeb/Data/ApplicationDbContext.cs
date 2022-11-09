using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Category> Categories { get; set; }
		public DbSet<ChangesLog> ChangesLogs { get; set; }
		public DbSet<Library> Libraries { get; set; }
		public DbSet<Sync> Syncs { get; set; }
		public DbSet<SyncLog> SyncLogs { get; set; }
	}
}