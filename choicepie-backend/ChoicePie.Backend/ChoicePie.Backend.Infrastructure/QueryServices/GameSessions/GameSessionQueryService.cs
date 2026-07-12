using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;

namespace ChoicePie.Backend.Infrastructure.QueryServices.GameSessions;

public sealed class GameSessionQueryService(IReadRepository readRepository) : IGameSessionQueryService, IScopedDependency
{
    public async Task<PagedResult<GameSessionSummaryDto>> GetHostedByUserIdAsync(
        Guid hostUserId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<GameSessionAggregate>()
            .Where(s => s.HostUserId == hostUserId)
            .OrderByDescending(s => s.PlayedAtUtc);

        return await PageAsync(query, pageNumber, pageSize, session => ToSummary(session, myMemberId: null));
    }

    public async Task<PagedResult<GameSessionSummaryDto>> GetPlayedByMemberIdAsync(
        Guid memberId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<GameSessionAggregate>()
            .Where(s => s.PlayerResults.Any(r => r.MemberId == memberId))
            .OrderByDescending(s => s.PlayedAtUtc);

        return await PageAsync(query, pageNumber, pageSize, session => ToSummary(session, memberId));
    }

    public Task<GameSessionDetailDto?> GetByIdAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken)
    {
        var session = readRepository.Query<GameSessionAggregate>().FirstOrDefault(s => s.Id == id);

        if (session is null)
        {
            return Task.FromResult<GameSessionDetailDto?>(null);
        }

        var isHost = session.HostUserId == requestingUserId;
        var myResult = session.PlayerResults.FirstOrDefault(r => r.MemberId == requestingUserId);

        var rankings = session.PlayerResults
            .OrderBy(r => r.Rank)
            .Select(r => new GameSessionRankEntryDto(r.Rank, r.Nickname, r.FinalScore))
            .ToList();

        var wrongAnswers = myResult is null
            ? []
            : BuildWrongAnswers(session, myResult);

        var dto = new GameSessionDetailDto(
            session.Id, session.RoomCode, session.QuizId, session.QuizTitle, session.CoverEmoji, session.CoverGradient,
            session.PlayedAtUtc, session.PlayerResults.Count, session.Questions.Count, isHost,
            rankings, myResult?.Rank, myResult?.FinalScore, wrongAnswers);

        return Task.FromResult<GameSessionDetailDto?>(dto);
    }

    private static IReadOnlyList<GameSessionWrongAnswerDto> BuildWrongAnswers(
        GameSessionAggregate session, Domain.Aggregates.GameSession.ValueObjects.GameSessionPlayerResult myResult)
    {
        var questionsByIndex = session.Questions
            .Select((q, index) => (Question: q, Index: index))
            .ToDictionary(x => x.Index, x => x.Question);

        return myResult.Answers
            .Where(a => !a.IsCorrect)
            .Where(a => questionsByIndex.ContainsKey(a.QuestionIndex))
            .Select(a =>
            {
                var question = questionsByIndex[a.QuestionIndex];
                return new GameSessionWrongAnswerDto(
                    question.Text, question.Options, a.SelectedOptionIndex, question.AnswerIndex, question.Explanation);
            })
            .ToList();
    }

    private static GameSessionSummaryDto ToSummary(GameSessionAggregate session, Guid? myMemberId)
    {
        var top = session.PlayerResults.OrderBy(r => r.Rank).FirstOrDefault();
        var mine = myMemberId is null ? null : session.PlayerResults.FirstOrDefault(r => r.MemberId == myMemberId);

        return new GameSessionSummaryDto(
            session.Id, session.RoomCode, session.QuizId, session.QuizTitle, session.CoverEmoji, session.CoverGradient,
            session.PlayedAtUtc, session.PlayerResults.Count, session.Questions.Count,
            top?.Nickname ?? string.Empty, top?.FinalScore ?? 0, mine?.Rank, mine?.FinalScore);
    }

    private static Task<PagedResult<GameSessionSummaryDto>> PageAsync(
        IOrderedQueryable<GameSessionAggregate> query, int pageNumber, int pageSize,
        Func<GameSessionAggregate, GameSessionSummaryDto> map)
    {
        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(map)
            .ToList();

        return Task.FromResult(new PagedResult<GameSessionSummaryDto>(items, pageNumber, pageSize, totalCount));
    }
}
