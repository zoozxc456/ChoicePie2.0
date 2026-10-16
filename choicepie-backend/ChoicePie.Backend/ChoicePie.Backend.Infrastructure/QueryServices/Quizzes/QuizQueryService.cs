using ChoicePie.Backend.Application.AdminDashboard.Dtos;
using ChoicePie.Backend.Application.AdminQuizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Quizzes;

public sealed class QuizQueryService(IReadRepository readRepository, TimeProvider timeProvider) : IQuizQueryService, IScopedDependency
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
                    q.Cover.ImageUrl,

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
                    q.Questions.Select(question => new QuestionStubDto(question.Id)).ToList(),
                    q.Questions.Count,
                    q.Tags,
                    q.ShareCount,
                    q.CreatedAt,
                    q.LastModifiedAt))
            .FirstOrDefault();

        return Task.FromResult(quiz);
    }

    public Task<AdminQuizDetailDto?> AdminGetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var quiz =
            (from q in readRepository.Query<Quiz>()
                where q.Id == id
                join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
                from creator in creatorGroup.DefaultIfEmpty()
                select new AdminQuizDetailDto(
                    q.Id,
                    q.Title,
                    q.Description,
                    q.Cover.ImageUrl,

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
                    q.Questions.Count,
                    q.Tags,
                    q.ShareCount,
                    readRepository.Query<QuizFavorite>().Count(f => f.QuizId == q.Id),
                    q.TakedownReason,
                    q.TakedownBy,
                    q.TakedownAt,
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
                    q.Cover.ImageUrl,

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
        var query = ownerId.HasValue
            ? readRepository.Query<Quiz>().Where(q => q.CreatorId == ownerId)
            : readRepository.Query<Quiz>().Where(q => q.Status == QuizStatus.Published);

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
                q.Cover.ImageUrl,

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
                q.Cover.ImageUrl,

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

    public Task<PagedResult<QuizSummaryDto>> ListFavoritesAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var ordered =
            from f in readRepository.Query<QuizFavorite>()
            where f.CreatorId == userId
            join q in readRepository.Query<Quiz>() on f.QuizId equals q.Id
            where q.Status == QuizStatus.Published
            orderby f.CreatedAt descending
            select q;

        return Task.FromResult(ToPagedSummaries(ordered, pageNumber, pageSize));
    }

    public Task<PagedResult<QuizSummaryDto>> ListFollowingFeedAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var followedCreatorIds = readRepository.Query<CreatorFollow>()
            .Where(f => f.CreatorId == userId)
            .Select(f => f.FollowedCreatorId);

        var ordered = readRepository.Query<Quiz>()
            .Where(q => q.Status == QuizStatus.Published && followedCreatorIds.Contains(q.CreatorId!.Value))
            .OrderByDescending(q => q.CreatedAt);

        return Task.FromResult(ToPagedSummaries(ordered, pageNumber, pageSize));
    }

    public Task<PagedResult<QuizSummaryDto>> ListByCreatorAsync(
        Guid creatorId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var ordered = readRepository.Query<Quiz>()
            .Where(q => q.CreatorId == creatorId && q.Status == QuizStatus.Published)
            .OrderByDescending(q => q.CreatedAt);

        return Task.FromResult(ToPagedSummaries(ordered, pageNumber, pageSize));
    }

    // 接收已排序的 Quiz 查詢，分頁後投影成 QuizSummaryDto（創作者資訊以相關子查詢取得，保留原排序）。
    private PagedResult<QuizSummaryDto> ToPagedSummaries(IQueryable<Quiz> ordered, int pageNumber, int pageSize)
    {
        var totalCount = ordered.Count();
        var members = readRepository.Query<Member>();

        var items = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuizSummaryDto(
                q.Id,
                q.Title,
                q.Description,
                q.Cover.ImageUrl,
                q.Cover.Emoji,
                q.Cover.Gradient,
                q.Difficulty.Name,
                q.Status.Name.ToLower(),
                q.Questions.Count,
                q.Stats.Count,
                q.Stats.PassRate,
                q.CreatorId!.Value,
                members.Where(m => m.Id == q.CreatorId).Select(m => m.Name).FirstOrDefault() ?? "Unknown",
                members.Where(m => m.Id == q.CreatorId).Select(m => m.Avatar).FirstOrDefault(),
                q.Tags,
                q.CreatedAt,
                q.LastModifiedAt))
            .ToList();

        return new PagedResult<QuizSummaryDto>(items, pageNumber, pageSize, totalCount);
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
                q.Cover.ImageUrl,

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

    public Task<AdminQuizDashboardStatsDto> AdminGetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sevenDaysAgo = now.AddDays(-7);

        var query = readRepository.Query<Quiz>();

        var totalCount = query.Count();
        var takenDownCount = query.Count(q => q.Status == QuizStatus.TakenDown);
        var newCountLast7Days = query.Count(q => q.CreatedAt >= sevenDaysAgo);
        var takenDownCountLast7Days = query.Count(q =>
            q.Status == QuizStatus.TakenDown && q.TakedownAt != null && q.TakedownAt >= sevenDaysAgo);

        var recentCreatedAts = query.Where(q => q.CreatedAt >= sevenDaysAgo).Select(q => q.CreatedAt).ToList();
        var newQuizzesByDay = Enumerable.Range(0, 7)
            .Select(offset => now.Date.AddDays(-6 + offset))
            .Select(day => new DailyCountDto(DateOnly.FromDateTime(day), recentCreatedAts.Count(d => d.Date == day)))
            .ToList();

        var recentTakedownAts = query
            .Where(q => q.Status == QuizStatus.TakenDown && q.TakedownAt != null && q.TakedownAt >= sevenDaysAgo)
            .Select(q => q.TakedownAt!.Value)
            .ToList();
        var takenDownQuizzesByDay = Enumerable.Range(0, 7)
            .Select(offset => now.Date.AddDays(-6 + offset))
            .Select(day => new DailyCountDto(DateOnly.FromDateTime(day), recentTakedownAts.Count(d => d.Date == day)))
            .ToList();

        return Task.FromResult(new AdminQuizDashboardStatsDto(
            totalCount, takenDownCount, newCountLast7Days, takenDownCountLast7Days,
            newQuizzesByDay, takenDownQuizzesByDay));
    }

    public Task<IReadOnlyList<QuizSummaryDto>> AdminGetTopQuizzesAsync(int limit, CancellationToken cancellationToken)
    {
        var joined =
            from q in readRepository.Query<Quiz>()
            where q.Status == QuizStatus.Published
            join m in readRepository.Query<Member>() on q.CreatorId!.Value equals m.Id into creatorGroup
            from creator in creatorGroup.DefaultIfEmpty()
            orderby q.Stats.Count descending, q.CreatedAt descending
            select new QuizSummaryDto(
                q.Id,
                q.Title,
                q.Description,
                q.Cover.ImageUrl,

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