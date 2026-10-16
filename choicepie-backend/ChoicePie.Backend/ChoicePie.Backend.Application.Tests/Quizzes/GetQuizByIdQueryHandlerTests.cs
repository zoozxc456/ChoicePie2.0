using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizByIdQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetQuizByIdQueryHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetQuizByIdQueryHandler(_quizQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_ownerId);
    }

    private static readonly QuestionDto Question = new(Guid.NewGuid(), "What is a Pod?", ["A", "B", "C", "D"], 0, "Explanation");

    private QuizDto MakeDto(Guid quizId, string status = "draft") => new(
        quizId, "Kubernetes 101", null, null, "⚓", "primary", "beginner", status, 0, 0, _ownerId, "Host Name", null,
        [Question], [new QuestionStubDto(Question.Id)], 1, [], 0,
        DateTime.UtcNow, DateTime.UtcNow);

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenReturnsQuiz()
    {
        var quizId = Guid.NewGuid();
        var dto = MakeDto(quizId);
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenServiceReturnsNull_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns((QuizDto?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenNonOwner_WhenCalled_ThenReturnsQuizWithoutQuestions()
    {
        var quizId = Guid.NewGuid();
        var dto = MakeDto(quizId, "published");
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var result = await _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None);

        Assert.That(result.Questions, Is.Empty);
        Assert.That(result.QuestionStubs, Is.Not.Empty);
        Assert.That(result.QuestionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_GivenAnonymousAndPublishedQuiz_WhenCalled_ThenReturnsStubsWithoutQuestions()
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(MakeDto(quizId, "published"));
        _currentUserService.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None);

        Assert.That(result.Questions, Is.Empty);
        Assert.That(result.QuestionStubs, Is.Not.Empty);
        Assert.That(result.QuestionCount, Is.EqualTo(1));
    }

    [TestCase("draft")]
    [TestCase("archived")]
    [TestCase("takendown")]
    [TestCase("deleted")]
    public void Handle_GivenAnonymousAndNonPublishedQuiz_WhenCalled_ThenThrowsQuizNotFoundException(string status)
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(MakeDto(quizId, status));
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNonOwnerAndDraftQuiz_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(MakeDto(quizId, "draft"));
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenOwnerAndArchivedQuiz_WhenCalled_ThenReturnsQuiz()
    {
        var quizId = Guid.NewGuid();
        var dto = MakeDto(quizId, "archived");
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }
}
