using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Data;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
	public DbSet<AppUser> Users => Set<AppUser>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<Message> Messages => Set<Message>();

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

		modelBuilder.Entity<Message>(entity =>
		{
			entity.HasKey(x => x.Id);
			entity.Property(x => x.Content).HasMaxLength(4000);
			entity.HasOne(x => x.Sender)
				.WithMany()
				.HasForeignKey(x => x.SenderId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasOne(x => x.Receiver)
				.WithMany()
				.HasForeignKey(x => x.ReceiverId)
				.OnDelete(DeleteBehavior.Restrict);
			entity.HasIndex(x => new { x.SenderId, x.ReceiverId, x.SentAt });
		});
	}
}
