using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Application.Comments.Queries;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Comments;

[TestFixture]
public class ListCommentsByQuizIdQueryHandlerTests
{
    private ICommentQueryService _commentQueryService = null!;
    private IQuizRepository _quizRepository = null!;
    private ListCommentsByQuizIdQueryHandler _sut = null!;
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _commentQueryService = Substitute.For<ICommentQueryService>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _sut = new ListCommentsByQuizIdQueryHandler(_commentQueryService, _quizRepository);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
    }

    [Test]
    public async Task Handle_GivenExistingQuiz_WhenCalled_ThenReturnsCommentsFromQueryService()
    {
        var expected = new List<CommentDto>
        {
            new(Guid.NewGuid(), _quiz.Id, Guid.NewGuid(), "Alice", null, "hi", DateTime.UtcNow)
        };
        _commentQueryService.ListByQuizIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListCommentsByQuizIdQuery(_quiz.Id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new ListCommentsByQuizIdQuery(missingId), CancellationToken.None));
    }
}
