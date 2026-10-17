namespace ChoicePie.Backend.Infrastructure.Caching.Quizzes;

// 題庫讀取快取的 key 與 tag。DTO 結構有不相容的改動時，把對應的版本號加一，
// rolling deploy 期間新舊版本就各自讀寫自己的 key，不會把對方的資料讀壞。
public static class QuizCacheKeys
{
    private const int QuizDtoVersion = 1;
    private const int QuizForAttemptDtoVersion = 1;
    private const int QuizSummaryDtoVersion = 1;
    private const int TagsVersion = 1;

    /// <summary>跨題庫的資料（標籤清單、相關題庫）共用的 tag。</summary>
    public const string CatalogTag = "quiz-catalog";

    public static string QuizTag(Guid quizId) => $"quiz:{quizId}";

    public static string Detail(Guid quizId) => $"quiz:{quizId}:detail:v{QuizDtoVersion}";

    public static string ForAttempt(Guid quizId) => $"quiz:{quizId}:attempt:v{QuizForAttemptDtoVersion}";

    public static string Related(Guid quizId, int limit) => $"quiz:{quizId}:related:{limit}:v{QuizSummaryDtoVersion}";

    public static string Tags => $"quiz-catalog:tags:v{TagsVersion}";
}
