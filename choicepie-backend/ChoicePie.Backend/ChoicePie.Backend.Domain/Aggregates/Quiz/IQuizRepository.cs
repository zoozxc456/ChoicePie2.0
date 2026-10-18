using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz;

public interface IQuizRepository : IRepository<Quiz>
{
    /// <summary>
    /// 分享數 +1 並回傳新的分享數；題庫不存在時回傳 null。
    /// 以單一 UPDATE 在資料庫原子遞增，同時分享不會因為「讀出來加一再寫回」而少算。
    /// </summary>
    Task<int?> IncrementShareCountAsync(Guid quizId, CancellationToken ct = default);

    /// <summary>
    /// 挑戰次數 +1 並依 <see cref="ValueObjects.ChallengeStats.RecordOutcome"/> 的公式更新通過率；題庫不存在時不做事。
    /// 以單一 UPDATE 在資料庫原子計算，很多人同時完成挑戰時不會少算。
    /// </summary>
    Task RecordChallengeOutcomeAsync(Guid quizId, bool passed, CancellationToken ct = default);
}
