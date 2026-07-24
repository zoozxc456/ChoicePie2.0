using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Quizzes;

public sealed class QuizQueryService(IReadRepository readRepository) : IQuizQueryService, IScopedDependency
{
    public Task<QuizDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var quiz =
            (from q in readRepository.Query<Quiz>()
                where q.Id == id
                join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
                from creator in creatorGroup.DefaultIfEmpty()
                select new QuizDto(
                    q.Id,
                    q.Title,
                    q.Description,
                    q.Cover.Emoji,
                    q.Cover.Gradient,
                    q.Difficulty.Name,
                    q.Status.Name.ToLower(),
                    q.Stats.Count,
                    q.Stats.PassRate,
                    q.CreatorId!.Value,
                    creator != null ? creator.Name : "Unknown",
                    creator != null ? creator.Avatar : null,
                    q.Questions.Select(question => new QuestionDto(question.Id, question.Text, question.Choices.Options,
                        question.Choices.AnswerIndex, question.Explanation)).ToList(),
                    q.Tags,
                    q.ShareCount,
                    q.CreatedAt,
                    q.LastModifiedAt))
            .FirstOrDefault();

        return Task.FromResult(quiz);
    }

    public Task<QuizForAttemptDto?> GetForAttemptAsync(Guid id, CancellationToken cancellationToken)
    {
        var quiz =
            (from q in readRepository.Query<Quiz>()
                where q.Id == id
                join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
                from creator in creatorGroup.DefaultIfEmpty()
                select new QuizForAttemptDto(
                    q.Id,
                    q.Title,
                    q.Description,
                    q.Cover.Emoji,
                    q.Cover.Gradient,
                    q.Difficulty.Name,
                    q.CreatorId!.Value,
                    creator != null ? creator.Name : "Unknown",
                    creator != null ? creator.Avatar : null,
                    q.Questions.Select(question =>
                        new QuestionForAttemptDto(question.Id, question.Text, question.Choices.Options)).ToList(),
                    q.Tags))
            .FirstOrDefault();

        return Task.FromResult(quiz);
    }

    public Task<PagedResult<QuizSummaryDto>> ListAsync(
        string? tag, string? search, Guid? ownerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query =
            readRepository.Query<Quiz>().Where(q =>
                q.Status == QuizStatus.Published || (q.CreatorId == ownerId && q.Status == QuizStatus.Draft));

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(q => q.Tags.Contains(tag));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q => q.Title.Contains(search));
        }

        var totalCount = query.Count();

        var joined =
            from q in query
            join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            orderby q.CreatedAt descending
            select new QuizSummaryDto(
                q.Id,
                q.Title,
                q.Description,
                q.Cover.Emoji,
                q.Cover.Gradient,
                q.Difficulty.Name,
                q.Status.Name.ToLower(),
                q.Questions.Count,
                q.Stats.Count,
                q.Stats.PassRate,
                q.CreatorId!.Value,
                creator != null ? creator.Name : "Unknown",
                creator != null ? creator.Avatar : null,
                q.Tags,
                q.CreatedAt,
                q.LastModifiedAt);

        var items = joined
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<QuizSummaryDto>(items, pageNumber, pageSize, totalCount));
    }

    public Task<PagedResult<QuizSummaryDto>> AdminListAsync(
        string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<Quiz>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q => q.Title.Contains(search));
        }

        var totalCount = query.Count();

        var joined =
            from q in query
            join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            orderby q.CreatedAt descending
            select new QuizSummaryDto(
                q.Id,
                q.Title,
                q.Description,
                q.Cover.Emoji,
                q.Cover.Gradient,
                q.Difficulty.Name,
                q.Status.Name.ToLower(),
                q.Questions.Count,
                q.Stats.Count,
                q.Stats.PassRate,
                q.CreatorId!.Value,
                creator != null ? creator.Name : "Unknown",
                creator != null ? creator.Avatar : null,
                q.Tags,
                q.CreatedAt,
                q.LastModifiedAt);

        var items = joined
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<QuizSummaryDto>(items, pageNumber, pageSize, totalCount));
    }

    public Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken)
    {
        var tags = readRepository.Query<Quiz>()
            .Where(q => q.Status == QuizStatus.Published)
            .SelectMany(q => q.Tags)
            .ToList()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(tags);
    }

    public Task<IReadOnlyList<QuizSummaryDto>> GetRelatedAsync(Guid quizId, int limit, CancellationToken cancellationToken)
    {
        var quiz = readRepository.Query<Quiz>().FirstOrDefault(q => q.Id == quizId);
        if (quiz == null || quiz.Tags.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<QuizSummaryDto>>([]);
        }

        var tags = quiz.Tags;

        var joined =
            from q in readRepository.Query<Quiz>()
            where q.Id != quizId && q.Status == QuizStatus.Published && q.Tags.Any(t => tags.Contains(t))
            join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            orderby q.Stats.Count descending, q.CreatedAt descending
            select new QuizSummaryDto(
                q.Id,
                q.Title,
                q.Description,
                q.Cover.Emoji,
                q.Cover.Gradient,
                q.Difficulty.Name,
                q.Status.Name.ToLower(),
                q.Questions.Count,
                q.Stats.Count,
                q.Stats.PassRate,
                q.CreatorId!.Value,
                creator != null ? creator.Name : "Unknown",
                creator != null ? creator.Avatar : null,
                q.Tags,
                q.CreatedAt,
                q.LastModifiedAt);

        var items = joined.Take(limit).ToList();

        return Task.FromResult<IReadOnlyList<QuizSummaryDto>>(items);
    }
}