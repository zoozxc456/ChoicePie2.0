using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;

public abstract class AppendOnlyAuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IAppendOnlyAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
    }
}