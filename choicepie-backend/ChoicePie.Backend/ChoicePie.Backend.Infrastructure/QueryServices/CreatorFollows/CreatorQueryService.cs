using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
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
}
