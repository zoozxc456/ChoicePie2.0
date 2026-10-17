namespace ChoicePie.Backend.Application.Quizzes.Contracts;

/// <summary>題庫讀取快取的失效埠；快取 key 與 tag 的細節由 Infrastructure 的實作決定。</summary>
public interface IQuizCacheInvalidator
{
    /// <summary>清掉單一題庫的詳情、作答用資料，以及以它為主的相關題庫清單。</summary>
    Task InvalidateQuizAsync(Guid quizId, CancellationToken cancellationToken);

    /// <summary>清掉跨題庫的資料（標籤清單、所有相關題庫清單）；題庫的可見狀態或標籤改變時呼叫。</summary>
    Task InvalidateCatalogAsync(CancellationToken cancellationToken);

    /// <summary>清掉含有該創作者名稱或頭像的題庫資料。</summary>
    Task InvalidateCreatorAsync(Guid creatorId, CancellationToken cancellationToken);
}
