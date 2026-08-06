using EarnTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EarnTrackerApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<IncomeSource> IncomeSources => Set<IncomeSource>();
    public DbSet<EarningTransaction> Transactions => Set<EarningTransaction>();
    public DbSet<FinancialGoal> FinancialGoals => Set<FinancialGoal>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(320);
            entity.Property(user => user.DisplayName).HasMaxLength(100);
            entity.Property(user => user.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<IncomeSource>(entity =>
        {
            entity.Property(source => source.Name).HasMaxLength(100);
            entity.Property(source => source.Provider).HasMaxLength(50);
            entity.Property(source => source.Currency).HasMaxLength(3);
            entity.HasOne(source => source.User)
                .WithMany(user => user.IncomeSources)
                .HasForeignKey(source => source.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EarningTransaction>(entity =>
        {
            entity.HasIndex(transaction => new
            {
                transaction.IncomeSourceId,
                transaction.ExternalId
            }).IsUnique();
            entity.Property(transaction => transaction.Amount).HasPrecision(18, 2);
            entity.Property(transaction => transaction.Fee).HasPrecision(18, 2);
            entity.Property(transaction => transaction.Currency).HasMaxLength(3);
            entity.Property(transaction => transaction.ExternalId).HasMaxLength(150);
            entity.Property(transaction => transaction.Status).HasMaxLength(30);
            entity.Property(transaction => transaction.Description).HasMaxLength(500);
            entity.HasOne(transaction => transaction.IncomeSource)
                .WithMany(source => source.Transactions)
                .HasForeignKey(transaction => transaction.IncomeSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialGoal>(entity =>
        {
            entity.Property(goal => goal.Name).HasMaxLength(100);
            entity.Property(goal => goal.Currency).HasMaxLength(3);
            entity.Property(goal => goal.TargetAmount).HasPrecision(18, 2);
            entity.HasOne(goal => goal.User)
                .WithMany(user => user.FinancialGoals)
                .HasForeignKey(goal => goal.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.Property(token => token.TokenHash).HasMaxLength(64);
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
