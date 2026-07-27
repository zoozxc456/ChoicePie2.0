using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Specifications;

public sealed class PendingQuizReportByQuizAndReporterSpecification(Guid quizId, Guid reporterId)
    : Specification<QuizReport>(r =>
        r.QuizId == quizId && r.ReporterId == reporterId && r.Status == ReportStatus.Pending);
