using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GenerateQuizQuestionsCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IQuizGenerationService _generationService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private GenerateQuizQuestionsCommandHandler _sut = null!;
    private Member _member = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _generationService = Substitute.For<IQuizGenerationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new GenerateQuizQuestionsCommandHandler(_memberRepository, _currentUserService, _generationService, _unitOfWork);

        _member = Member.Register(Email.Create("host@example.com"), "Host Name", "hashed");
        _currentUserService.UserId.Returns(_member.Id);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private static GenerateQuizQuestionsCommand ValidCommand() => new()
    {
        Content = new string('x', 30),
        QuestionCount = 5,
        Difficulty = "beginner"
    };

    [Test]
    public async Task Handle_GivenValidRequest_WhenCalled_ThenReturnsGeneratedQuestionsAndRecordsUsage()
    {
        _generationService.GenerateAsync(Arg.Any<string>(), 5, Difficulty.Beginner, Arg.Any<CancellationToken>())
            .Returns(new GeneratedQuestionsResult(
                [new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 42));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TokensUsed, Is.EqualTo(42));
            Assert.That(result.Questions, Has.Count.EqualTo(1));
        });
        Assert.That(_member.CanGenerateQuizToday(DateTime.UtcNow), Is.False);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuotaAlreadyUsedToday_WhenCalled_ThenThrowsAiGenerationQuotaExceededException()
    {
        _member.RecordAiGeneration(DateTime.UtcNow);

        Assert.ThrowsAsync<AiGenerationQuotaExceededException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(7)]
    public void Handle_GivenDisallowedQuestionCount_WhenCalled_ThenThrowsInvalidQuizException(int count)
    {
        var command = new GenerateQuizQuestionsCommand
        {
            Content = new string('x', 30), QuestionCount = count, Difficulty = "beginner"
        };

        Assert.ThrowsAsync<InvalidQuizException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnknownDifficulty_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var command = new GenerateQuizQuestionsCommand
        {
            Content = new string('x', 30), QuestionCount = 5, Difficulty = "legendary"
        };

        Assert.ThrowsAsync<InvalidQuizException>(() => _sut.Handle(command, CancellationToken.None));
    }
}
