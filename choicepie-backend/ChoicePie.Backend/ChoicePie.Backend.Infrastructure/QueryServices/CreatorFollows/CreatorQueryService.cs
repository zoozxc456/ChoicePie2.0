using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.CreatorFollows;

public sealed class CreatorQueryService(IReadRepository readRepository) : ICreatorQueryService, IScopedDependency
{
    public Task<CreatorProfileDto?> GetByIdAsync(Guid creatorId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var creator = readRepository.Query<Member>().FirstOrDefault(m => m.Id == creatorId);

        if (creator is null)
        {
            return Task.FromResult<CreatorProfileDto?>(null);
        }

        var publishedQuizzes = readRepository.Query<Quiz>()
            .Where(q => q.CreatorId == creatorId && q.Status == QuizStatus.Published)
            .ToList();

        var quizCount = publishedQuizzes.Count;
        var challengeCount = publishedQuizzes.Sum(q => q.ChallengeCount);

        var isFollowing = currentUserId.HasValue &&
            readRepository.Query<CreatorFollow>()
                .Any(f => f.CreatorId == currentUserId.Value && f.FollowedCreatorId == creatorId);

        return Task.FromResult<CreatorProfileDto?>(new CreatorProfileDto(
            creator.Id, creator.Name, creator.Avatar, quizCount, challengeCount, isFollowing));
    }

    public Task<PagedResult<FollowedCreatorDto>> ListFollowingAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var follows =
            from f in readRepository.Query<CreatorFollow>()
            where f.CreatorId == userId
            join m in readRepository.Query<Member>() on f.FollowedCreatorId equals m.Id
            select new { Follow = f, Creator = m };

        var totalCount = follows.Count();
        var quizzes = readRepository.Query<Quiz>();

        var items = follows
            .OrderByDescending(x => x.Follow.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FollowedCreatorDto(
                x.Creator.Id,
                x.Creator.Name,
                x.Creator.Avatar,
                quizzes.Count(q => q.CreatorId == x.Creator.Id && q.Status == QuizStatus.Published),
                x.Follow.CreatedAt))
            .ToList();

        return Task.FromResult(new PagedResult<FollowedCreatorDto>(items, pageNumber, pageSize, totalCount));
    }
}
