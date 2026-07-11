using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed record GetQuizByIdQuery(Guid Id) : IRequest<QuizDto>;
