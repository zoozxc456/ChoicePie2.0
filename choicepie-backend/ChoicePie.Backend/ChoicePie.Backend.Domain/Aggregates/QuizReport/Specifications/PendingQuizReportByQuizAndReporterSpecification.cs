using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Specifications;

public sealed class PendingQuizReportByQuizAndReporterSpecification(Guid quizId, Guid reporterId)
    : Specification<QuizReport>(r =>
        // ReporterId 是由稽核欄位 CreatorId 算出來的 [NotMapped] 屬性，EF 無法轉譯，必須直接比對 CreatorId。
        r.QuizId == quizId && r.CreatorId == reporterId && r.Status == ReportStatus.Pending);
