using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Commands;

public sealed class AdminResolveQuizReportCommandHandler(
    IQuizReportRepository quizReportRepository,
    IQuizRepository quizRepository,
    ICurrentAdminUserService currentAdminUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AdminResolveQuizReportCommand>
{
    public async Task Handle(AdminResolveQuizReportCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();

        var report = await quizReportRepository.GetByIdAsync(request.ReportId, cancellationToken)
                     ?? throw new QuizReportNotFoundException(request.ReportId);

        var quiz = await quizRepository.GetByIdAsync(report.QuizId, cancellationToken)
                   ?? throw new QuizNotFoundException(report.QuizId);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var takedownReason = string.IsNullOrWhiteSpace(request.Note) ? "違反社群規範，經檢舉審核後下架。" : request.Note;

        report.Resolve(adminUserId, request.Note, utcNow);
        if (quiz.Status != QuizStatus.TakenDown)
        {
            quiz.TakeDown(adminUserId, takedownReason, utcNow);
        }

        await quizReportRepository.UpdateAsync(report, cancellationToken);
        await quizRepository.UpdateAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
