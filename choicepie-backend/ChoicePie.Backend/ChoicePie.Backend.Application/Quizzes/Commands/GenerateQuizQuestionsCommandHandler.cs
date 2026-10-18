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
    IWebPageContentFetcher webPageContentFetcher,
    IOptions<AiQuizGenerationSettings> aiSettings,
    IUnitOfWork unitOfWork,
    IDistributedLock distributedLock,
    TimeProvider timeProvider)
    : IRequestHandler<GenerateQuizQuestionsCommand, GenerateQuestionsResultDto>
{
    // 要涵蓋抓網頁 + AI 呼叫的最長時間（HttpClient 預設 100 秒逾時）；請求結束時就會提早釋放。
    private static readonly TimeSpan AiGenerationLease = TimeSpan.FromMinutes(5);

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

        // 額度是「先檢查、AI 回來後才記錄用量」，中間隔著好幾秒的 AI 呼叫：同一位會員的並行請求
        // 會一起通過檢查而超用付費 API。同一位會員同時只放行一個出題請求（不排隊，直接回 409）。
        await using var inFlight = await distributedLock.TryAcquireAsync(
                                       $"AiGeneration:{memberId}", AiGenerationLease, cancellationToken)
                                   ?? throw new AiGenerationInProgressException(memberId);
        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);

        // 未指派方案視同額度為 0。
        var tierId = member.TierId ?? throw new AiGenerationNotAvailableException(memberId);
        var tier = await membershipTierRepository.GetByIdAsync(tierId, cancellationToken)
                   ?? throw new MembershipTierNotFoundException(tierId);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var todayUsage = await aiUsageLogQueryService.GetTodayUsageAsync(memberId, now, cancellationToken);

        if (todayUsage.GenerationCount >= tier.DailyGenerationLimit)
        {
            throw new AiGenerationQuotaExceededException(memberId);
        }

        if (todayUsage.TokensUsed >= tier.DailyTokenBudget)
        {
            throw new AiTokenBudgetExceededException(memberId);
        }

        // 先抓取網頁，失敗就直接丟出例外：此時尚未記錄任何用量，不會消耗額度。
        var (content, source) = await ResolveContentAsync(request.Content, cancellationToken);

        var result = await generationService.GenerateAsync(
            content, request.QuestionCount, difficulty, cancellationToken, source);

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

    /// <summary>
    /// 輸入為單一 http/https 網址時抓取網頁文字（截斷至上限並重新檢查最小長度）；純文字原樣使用。
    /// </summary>
    private async Task<(string Content, WebPageSource? Source)> ResolveContentAsync(
        string input, CancellationToken cancellationToken)
    {
        var url = QuizContentInput.ParseUrlOrNull(input);
        if (url is null)
        {
            return (input, null);
        }

        var page = await webPageContentFetcher.FetchAsync(url, cancellationToken);

        var text = page.Text.Trim();
        if (text.Length > QuizContentInput.MaxContentLength)
        {
            text = text[..QuizContentInput.MaxContentLength];
        }

        if (text.Length < QuizContentInput.MinContentLength)
        {
            throw new WebPageContentInsufficientException(text.Length);
        }

        return (text, new WebPageSource(page.Url, page.Title));
    }
}
