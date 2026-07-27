using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizByIdQueryHandler(IQuizQueryService quizQueryService, ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizByIdQuery, QuizDto>
{
    public async Task<QuizDto> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var quiz = await quizQueryService.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new QuizNotFoundException(request.Id);

        // Non-owners can see basic info and ratings but not question content - Questions is
        // stripped here rather than throwing, so browsing someone else's quiz works.
        return quiz.CreatorId == userId ? quiz : quiz with { Questions = [] };
    }
}
