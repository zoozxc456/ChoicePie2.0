using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizForAttemptQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<GetQuizForAttemptQuery, QuizForAttemptDto>
{
    public async Task<QuizForAttemptDto> Handle(GetQuizForAttemptQuery request, CancellationToken cancellationToken) =>
        await quizQueryService.GetForAttemptAsync(request.Id, cancellationToken)
        ?? throw new QuizNotFoundException(request.Id);
}
