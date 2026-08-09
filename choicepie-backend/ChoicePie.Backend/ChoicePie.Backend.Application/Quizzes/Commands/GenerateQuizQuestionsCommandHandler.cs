using ChoicePie.Backend.Application.AiUsage.Contracts;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class GenerateQuizQuestionsCommandHandler(
    IMemberRepository memberRepository,
    IMembershipTierRepository membershipTierRepository,
    IAiUsageLogRepository aiUsageLogRepository,
    IAiUsageLogQueryService aiUsageLogQueryService,
    ICurrentUserService currentUserService,
    IQuizGenerationService generationService,
    IOptions<AiQuizGenerationSettings> aiSettings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<GenerateQuizQuestionsCommand, GenerateQuestionsResultDto>
{
    private static readonly int[] AllowedQuestionCounts = [3, 5, 10];

    public async Task<GenerateQuestionsResultDto> Handle(
        GenerateQuizQuestionsCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedQuestionCounts.Contains(request.QuestionCount))
        {
            throw new InvalidQuizException("題目數量必須為 3、5 或 10。");
        }

        var difficulty = Difficulty.FromName(request.Difficulty)
                          ?? throw new InvalidQuizException($"未知的難度：{request.Difficulty}");

        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);

        var tier = member.TierId is { } tierId
            ? await membershipTierRepository.GetByIdAsync(tierId, cancellationToken)
              ?? throw new MembershipTierNotFoundException(tierId)
            : null;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (tier is not null)
        {
            var todayUsage = await aiUsageLogQueryService.GetTodayUsageAsync(memberId, now, cancellationToken);

            if (todayUsage.GenerationCount >= tier.DailyGenerationLimit)
            {
                throw new AiGenerationQuotaExceededException(memberId);
            }

            if (todayUsage.TokensUsed >= tier.DailyTokenBudget)
            {
                throw new AiTokenBudgetExceededException(memberId);
            }
        }

        var result = await generationService.GenerateAsync(
            request.Content, request.QuestionCount, difficulty, cancellationToken);

        member.RecordAiGeneration(now);
        await memberRepository.UpdateAsync(member, cancellationToken);

        var usageLog = AiUsageLog.Create(memberId, aiSettings.Value.Provider, aiSettings.Value.Model, result.TokensUsed);
        await aiUsageLogRepository.AddAsync(usageLog, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var questions = result.Questions
            .Select(q => new GeneratedQuestionDto(q.Text, q.Options, q.AnswerIndex, q.Explanation))
            .ToList();

        return new GenerateQuestionsResultDto(questions, result.TokensUsed);
    }
}
