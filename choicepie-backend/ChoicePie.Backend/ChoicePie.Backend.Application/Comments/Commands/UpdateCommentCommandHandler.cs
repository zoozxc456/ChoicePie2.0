using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed class UpdateCommentCommandHandler(
    ICommentRepository commentRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw new CommentNotFoundException(request.Id);

        comment.EnsureModifiableBy(userId);
        comment.UpdateText(request.Text);

        await commentRepository.UpdateAsync(comment, cancellationToken);
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
