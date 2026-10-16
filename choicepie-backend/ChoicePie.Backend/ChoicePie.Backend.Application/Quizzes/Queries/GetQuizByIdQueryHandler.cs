using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizByIdQueryHandler(IQuizQueryService quizQueryService, ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizByIdQuery, QuizDto>
{
    public async Task<QuizDto> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken)
    {
        // 匿名訪客（分享連結、社群爬蟲）也能呼叫，userId 為 null 時視同非擁有者。
        var userId = currentUserService.UserId;
        var quiz = await quizQueryService.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new QuizNotFoundException(request.Id);

        var isOwner = userId is not null && quiz.CreatorId == userId;

        // 非擁有者（含匿名）只能看到已發佈的題庫，其餘狀態一律視為不存在，避免洩漏草稿/下架內容。
        if (!isOwner && !string.Equals(quiz.Status, QuizStatus.Published.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new QuizNotFoundException(request.Id);
        }

        // Non-owners can see basic info and ratings but not question content - Questions is
        // stripped here rather than throwing, so browsing someone else's quiz works.
        return isOwner ? quiz : quiz with { Questions = [] };
    }
}
