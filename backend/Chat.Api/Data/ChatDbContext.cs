using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Data;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
	public DbSet<AppUser> Users => Set<AppUser>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AppUser>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Email).HasMaxLength(320);
			entity.HasIndex(x => x.Email).IsUnique();
		});
	}
}
