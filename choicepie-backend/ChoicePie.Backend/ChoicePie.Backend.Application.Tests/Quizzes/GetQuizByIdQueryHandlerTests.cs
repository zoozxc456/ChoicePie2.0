using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizByIdQueryHandlerTests
{
    private IReadRepository _readRepository = null!;
    private GetQuizByIdQueryHandler _sut = null!;
    private Quiz _quiz = null!;
    private Member _creator = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new GetQuizByIdQueryHandler(_readRepository);

        _creator = Member.Create("Host Name");
        _quiz = Quiz.Create(_creator.Id, "Kubernetes 101", null, "⚓", "g", Difficulty.Beginner, true, ["Kubernetes"]);

        _readRepository.Query<Quiz>().Returns(new List<Quiz> { _quiz }.AsQueryable());
        _readRepository.Query<Member>().Returns(new List<Member> { _creator }.AsQueryable());
    }

    [Test]
    public async Task Handle_GivenExistingQuiz_WhenCalled_ThenReturnsQuizDtoWithCreatorInfo()
    {
        var result = await _sut.Handle(new GetQuizByIdQuery(_quiz.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Kubernetes 101"));
            Assert.That(result.CreatorName, Is.EqualTo("Host Name"));
        });
    }

    [Test]
    public void Handle_GivenUnknownId_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
