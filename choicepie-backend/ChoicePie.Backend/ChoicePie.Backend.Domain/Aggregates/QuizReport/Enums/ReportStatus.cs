using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;

public sealed record ReportStatus(int Id, string Name) : Enumeration<ReportStatus>(Id, Name)
{
    public static readonly ReportStatus Pending = new(1, nameof(Pending));
    public static readonly ReportStatus Resolved = new(2, nameof(Resolved));
    public static readonly ReportStatus Dismissed = new(3, nameof(Dismissed));
}
