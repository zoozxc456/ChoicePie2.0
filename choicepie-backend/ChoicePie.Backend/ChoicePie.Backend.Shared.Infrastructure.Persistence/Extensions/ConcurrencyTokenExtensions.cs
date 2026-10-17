using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

public static class ConcurrencyTokenExtensions
{
    extension<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : class
    {
        /// <summary>
        /// 以 Postgres 每列都有的系統欄位 xmin 做樂觀並行控制：不需要新增實體欄位，Domain 也看不到它。
        /// 讀取後被別的交易改過的列，UPDATE 會影響 0 列，EfUnitOfWork 轉成 ConcurrencyConflictException。
        /// </summary>
        public EntityTypeBuilder<TEntity> UseXminConcurrencyToken()
        {
            builder.Property<uint>("Version").IsRowVersion();
            return builder;
        }
    }
}
