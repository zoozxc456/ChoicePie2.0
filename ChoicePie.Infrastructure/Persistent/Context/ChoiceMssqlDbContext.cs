using ChoicePie.Domain.Aggregates.UserAggregate;
using ChoicePie.Infrastructure.Persistent.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Infrastructure.Persistent.Context;

public class ChoiceMssqlDbContext(DbContextOptions<ChoiceMssqlDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserInfoEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserPasswordEntityTypeConfiguration());
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<UserPassword> UserPasswords => Set<UserPassword>();
}