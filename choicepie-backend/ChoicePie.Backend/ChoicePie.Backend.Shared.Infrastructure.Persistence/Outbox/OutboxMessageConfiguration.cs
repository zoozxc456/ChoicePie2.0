using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Type).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired();

        // 認領查詢只看還在排隊的訊息；dead letter 不進索引。
        builder.HasIndex(m => m.NextAttemptAt).HasFilter("dead_lettered_at IS NULL");
    }
}

public static class OutboxModelBuilderExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        /// <summary>每個交給 EfUnitOfWork 的 DbContext 都要呼叫：事件會跟業務資料寫進同一個 context。</summary>
        public ModelBuilder ApplyOutboxConfiguration() =>
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
