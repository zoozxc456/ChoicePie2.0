using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class CreateQuizCommandHandlerTests
{
    private IRepository<Quiz> _quizRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private CreateQuizCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizRepository = Substitute.For<IRepository<Quiz>>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateQuizCommandHandler(_quizRepository, _memberRepository, _currentUserService, _unitOfWork);

        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private static CreateQuizCommand ValidCommand() => new()
    {
        Title = "Kubernetes 101",
        Description = "A quiz",
        CoverEmoji = "⚓",
        CoverGradient = "gradient",
        Difficulty = "beginner",
        IsPublic = true,
        Tags = ["Kubernetes"],
        Questions =
        [
            new CreateQuestionDto("2+2=?", ["1", "2", "3", "4"], 3, "basic math")
        ]
    };

    [Test]
    public async Task Handle_GivenValidInput_WhenCalled_ThenPersistsQuizWithQuestions()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _quizRepository.Received(1).AddAsync(
            Arg.Is<Quiz>(q => q.Title == "Kubernetes 101" && q.QuestionCount == 1 && q.OwnerId == _userId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenValidInput_WhenCalled_ThenReturnsQuizDtoWithCreatorInfo()
    {
        var creator = Member.Register(Email.Create("host@example.com"), "Host Name", "hashed");
        _memberRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(creator);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Kubernetes 101"));
            Assert.That(result.CreatorName, Is.EqualTo("Host Name"));
            Assert.That(result.Questions, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Handle_GivenUnknownDifficulty_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var command = ValidCommand();
        command = new CreateQuizCommand
        {
            Title = command.Title,
            Description = command.Description,
            CoverEmoji = command.CoverEmoji,
            CoverGradient = command.CoverGradient,
            Difficulty = "legendary",
            IsPublic = command.IsPublic,
            Tags = command.Tags,
            Questions = command.Questions
        };

        Assert.ThrowsAsync<InvalidQuizException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
