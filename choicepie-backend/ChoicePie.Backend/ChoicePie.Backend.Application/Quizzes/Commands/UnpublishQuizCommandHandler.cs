using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class UnpublishQuizCommandHandler(
    IQuizRepository quizRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnpublishQuizCommand, QuizDto>
{
    public async Task<QuizDto> Handle(UnpublishQuizCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var quiz = await quizRepository.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new QuizNotFoundException(request.Id);

        quiz.EnsureModifiableBy(userId);
        quiz.Unpublish();

        await quizRepository.UpdateAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var creator = await memberRepository.GetByIdAsync(userId, cancellationToken);

        return QuizDto.FromDomain(quiz, creator?.Name ?? "Unknown", creator?.Avatar);
    }
}
