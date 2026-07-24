using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport;

public sealed class QuizReport : AggregateRoot<Guid>
{
    private const int MaxDescriptionLength = 500;
    private const int MaxResolutionNoteLength = 500;

    public Guid QuizId { get; private set; }
    public ReportReason Reason { get; private set; } = null!;
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; } = null!;
    public Guid ReporterId => CreatorId!.Value;

    public Guid? ResolvedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolutionNote { get; private set; }

    private QuizReport()
    {
    }

    public static QuizReport Create(Guid quizId, Guid reporterId, ReportReason reason, string? description)
    {
        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedDescription is { Length: > MaxDescriptionLength })
        {
            throw new InvalidQuizReportException($"檢舉說明不可超過 {MaxDescriptionLength} 字。");
        }

        var report = new QuizReport
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Reason = reason,
            Description = trimmedDescription,
            Status = ReportStatus.Pending
        };

        report.SetCreated(reporterId);

        return report;
    }

    public void Resolve(Guid adminId, string? note, DateTime utcNow)
    {
        EnsurePending();
        ValidateResolutionNote(note);

        Status = ReportStatus.Resolved;
        ResolvedBy = adminId;
        ResolvedAt = utcNow;
        ResolutionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    public void Dismiss(Guid adminId, string? note, DateTime utcNow)
    {
        EnsurePending();
        ValidateResolutionNote(note);

        Status = ReportStatus.Dismissed;
        ResolvedBy = adminId;
        ResolvedAt = utcNow;
        ResolutionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    private void EnsurePending()
    {
        if (Status != ReportStatus.Pending)
        {
            throw new InvalidQuizReportException("此檢舉已經處理過。");
        }
    }

    private static void ValidateResolutionNote(string? note)
    {
        if (note is { Length: > MaxResolutionNoteLength })
        {
            throw new InvalidQuizReportException($"處理備註不可超過 {MaxResolutionNoteLength} 字。");
        }
    }
}
