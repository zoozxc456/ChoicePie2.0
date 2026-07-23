using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Queries;

public sealed class ListCommentsByQuizIdQueryHandler(
    ICommentQueryService commentQueryService,
    IQuizRepository quizRepository)
    : IRequestHandler<ListCommentsByQuizIdQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(ListCommentsByQuizIdQuery request, CancellationToken cancellationToken)
    {
        _ = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
            ?? throw new QuizNotFoundException(request.QuizId);

        return await commentQueryService.ListByQuizIdAsync(request.QuizId, cancellationToken);
    }
}
