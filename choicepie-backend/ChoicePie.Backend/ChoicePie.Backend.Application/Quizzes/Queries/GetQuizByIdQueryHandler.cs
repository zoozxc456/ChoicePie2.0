using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizByIdQueryHandler(IReadRepository readRepository)
    : IRequestHandler<GetQuizByIdQuery, QuizDto>
{
    public Task<QuizDto> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken)
    {
        var quiz = readRepository.Query<Quiz>().FirstOrDefault(q => q.Id == request.Id)
                   ?? throw new QuizNotFoundException(request.Id);

        var creator = readRepository.Query<Member>().FirstOrDefault(m => m.Id == quiz.OwnerId);

        return Task.FromResult(QuizDto.FromDomain(quiz, creator?.Name ?? "Unknown", creator?.Avatar));
    }
}
