using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizByIdQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<GetQuizByIdQuery, QuizDto>
{
    public async Task<QuizDto> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken) =>
        await quizQueryService.GetByIdAsync(request.Id, cancellationToken)
        ?? throw new QuizNotFoundException(request.Id);
}
