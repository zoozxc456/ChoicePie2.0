using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class CreateQuizCommandHandler(
    IRepository<Quiz> quizRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateQuizCommand, QuizDto>
{
    public async Task<QuizDto> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var difficulty = Difficulty.FromName(request.Difficulty)
                          ?? throw new InvalidQuizException($"未知的難度：{request.Difficulty}");

        var quiz = Quiz.Create(
            userId, request.Title, request.Description, request.CoverEmoji, request.CoverGradient,
            difficulty, request.IsPublic, request.Tags);

        foreach (var question in request.Questions)
        {
            quiz.AddQuestion(Question.Create(question.Text, question.Options, question.AnswerIndex, question.Explanation));
        }

        await quizRepository.AddAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var creator = await memberRepository.GetByIdAsync(userId, cancellationToken);

        return QuizDto.FromDomain(quiz, creator?.Name ?? "Unknown", creator?.Avatar);
    }
}
