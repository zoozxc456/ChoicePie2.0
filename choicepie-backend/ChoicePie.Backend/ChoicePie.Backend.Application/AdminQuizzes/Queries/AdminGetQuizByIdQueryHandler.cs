using ChoicePie.Backend.Application.AdminQuizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed class AdminGetQuizByIdQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<AdminGetQuizByIdQuery, AdminQuizDetailDto>
{
    public async Task<AdminQuizDetailDto> Handle(AdminGetQuizByIdQuery request, CancellationToken cancellationToken) =>
        await quizQueryService.AdminGetByIdAsync(request.Id, cancellationToken)
        ?? throw new QuizNotFoundException(request.Id);
}
