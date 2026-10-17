using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizReport.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.QuizReports.Commands;

public sealed class CreateQuizReportCommandHandler(
    IQuizReportRepository quizReportRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQuizReportCommand>
{
    public async Task Handle(CreateQuizReportCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        _ = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
            ?? throw new QuizNotFoundException(request.QuizId);

        var reason = ReportReason.FromName(request.Reason)
                     ?? throw new InvalidQuizReportException("無效的檢舉原因。");

        var alreadyReported = await quizReportRepository.ExistsAsync(
            new PendingQuizReportByQuizAndReporterSpecification(request.QuizId, userId), cancellationToken);

        if (alreadyReported)
        {
            throw new InvalidQuizReportException("你已檢舉過此題庫，請等待處理結果。");
        }

        var report = QuizReport.Create(request.QuizId, userId, reason, request.Description);

        await quizReportRepository.AddAsync(report, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // 連點檢舉：同一份題庫最多一筆待處理檢舉，由部分唯一索引擋下較慢的那個。
            throw new InvalidQuizReportException("你已檢舉過此題庫，請等待處理結果。");
        }
    }
}
