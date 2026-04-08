using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Data;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
	public DbSet<AppUser> Users => Set<AppUser>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AppUser>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Email).HasMaxLength(320);
			entity.HasIndex(x => x.Email).IsUnique();
		});

		modelBuilder.Entity<RefreshToken>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.HasIndex(x => x.TokenHash).IsUnique();
			entity.Property(x => x.TokenHash).HasMaxLength(128);
			entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
		});
	}
}
