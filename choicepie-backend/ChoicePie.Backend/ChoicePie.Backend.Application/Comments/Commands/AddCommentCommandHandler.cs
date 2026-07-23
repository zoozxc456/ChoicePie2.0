using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed class AddCommentCommandHandler(
    ICommentRepository commentRepository,
    IQuizRepository quizRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        _ = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
            ?? throw new QuizNotFoundException(request.QuizId);

        var comment = Comment.Create(request.QuizId, userId, request.Text);
        await commentRepository.AddAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var author = await memberRepository.GetByIdAsync(userId, cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.QuizId,
            comment.UserId,
            author?.Name ?? "Unknown",
            author?.Avatar,
            comment.Text,
            comment.CreatedAt);
    }
}
