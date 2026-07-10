using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;

public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.LastModifiedAt).IsRequired();

        // EF Core query filters don't compose - a subclass calling HasQueryFilter again will
        // silently overwrite this one and must repeat the `!e.DeletedAt.HasValue` predicate itself.
        // (IAuditableEntity only exposes DeletedAt, not the concrete class's IsDeleted, hence the
        // equivalent check here rather than referencing IsDeleted directly.)
        builder.HasQueryFilter(e => !e.DeletedAt.HasValue);
    }
}