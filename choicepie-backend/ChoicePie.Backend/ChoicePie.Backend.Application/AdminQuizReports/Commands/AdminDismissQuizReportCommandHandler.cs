using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizReports.Commands;

public sealed class AdminDismissQuizReportCommandHandler(
    IQuizReportRepository quizReportRepository,
    ICurrentAdminUserService currentAdminUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AdminDismissQuizReportCommand>
{
    public async Task Handle(AdminDismissQuizReportCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();

        var report = await quizReportRepository.GetByIdAsync(request.ReportId, cancellationToken)
                     ?? throw new QuizReportNotFoundException(request.ReportId);

        report.Dismiss(adminUserId, request.Note, timeProvider.GetUtcNow().UtcDateTime);

        await quizReportRepository.UpdateAsync(report, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
