using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;

public static class AdvisoryLockExtensions
{
    extension(DatabaseFacade database)
    {
        /// <summary>
        /// 取得 Postgres session 層級的 advisory lock，等到拿到為止；DisposeAsync 時釋放。
        /// 用獨立且不進 pool 的連線持有鎖：連線一斷鎖就跟著釋放，持有者當機也不會卡住其他節點；
        /// 也不會因為連線回到 pool 而讓鎖殘留在別人借走的連線上。
        /// </summary>
        public async Task<IAsyncDisposable> AcquireAdvisoryLockAsync(long lockKey, CancellationToken ct = default)
        {
            var connectionString = new NpgsqlConnectionStringBuilder(database.GetConnectionString())
            {
                Pooling = false
            }.ConnectionString;

            var connection = new NpgsqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(ct);
                await using var command = new NpgsqlCommand("SELECT pg_advisory_lock(@key)", connection);
                command.Parameters.AddWithValue("key", lockKey);
                await command.ExecuteNonQueryAsync(ct);
                return new AdvisoryLock(connection, lockKey);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }
    }

    private sealed class AdvisoryLock(NpgsqlConnection connection, long lockKey) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await using (connection)
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", connection);
                command.Parameters.AddWithValue("key", lockKey);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
