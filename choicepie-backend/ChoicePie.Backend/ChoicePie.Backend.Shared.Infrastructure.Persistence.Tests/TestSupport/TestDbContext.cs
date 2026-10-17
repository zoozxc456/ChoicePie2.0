using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TestWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new TestGadgetConfiguration());
    }
}

public sealed class TestWidgetConfiguration : AuditableEntityConfiguration<TestWidget>
{
    public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TestWidget> builder)
    {
        base.Configure(builder);

        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
    }
}

public sealed class TestGadgetConfiguration : AuditableEntityConfiguration<TestGadget>
{
    public override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TestGadget> builder)
    {
        base.Configure(builder);

        builder.Property(g => g.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(g => g.Code).IsUnique();
        builder.Property(g => g.Label).IsRequired().HasMaxLength(50);
        builder.UseXminConcurrencyToken();
    }
}
