using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Queries;

public sealed record ListMyFavoritesQuery(int PageNumber, int PageSize) : IRequest<PagedResult<QuizSummaryDto>>;
