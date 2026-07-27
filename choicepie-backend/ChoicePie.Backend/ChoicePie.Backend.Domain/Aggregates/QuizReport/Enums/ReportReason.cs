using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;

public sealed record ReportReason(int Id, string Name) : Enumeration<ReportReason>(Id, Name)
{
    public static readonly ReportReason InappropriateContent = new(1, nameof(InappropriateContent));
    public static readonly ReportReason Spam = new(2, nameof(Spam));
    public static readonly ReportReason Copyright = new(3, nameof(Copyright));
    public static readonly ReportReason Other = new(4, nameof(Other));
}
