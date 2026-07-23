using ChoicePie.Backend.Application.QuizFavorites.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizFavorites;

[TestFixture]
public class AddQuizFavoriteCommandHandlerTests
{
    private IQuizFavoriteRepository _favoriteRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AddQuizFavoriteCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private Quiz _quiz = null!;

    [SetUp]
    public void SetUp()
    {
        _favoriteRepository = Substitute.For<IQuizFavoriteRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AddQuizFavoriteCommandHandler(_favoriteRepository, _quizRepository, _currentUserService, _unitOfWork);

        _quiz = Quiz.Create(Guid.NewGuid(), "Title", null, "⚓", "g", Difficulty.Beginner, []);
        _quizRepository.GetByIdAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);
        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenNotAlreadyFavorited_WhenCalled_ThenAddsFavoriteAndPersists()
    {
        _favoriteRepository.ExistsAsync(Arg.Any<ISpecification<QuizFavorite>>(), Arg.Any<CancellationToken>()).Returns(false);

        await _sut.Handle(new AddQuizFavoriteCommand(_quiz.Id), CancellationToken.None);

        await _favoriteRepository.Received(1).AddAsync(Arg.Any<QuizFavorite>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenAlreadyFavorited_WhenCalled_ThenDoesNotAddOrPersistAgain()
    {
        _favoriteRepository.ExistsAsync(Arg.Any<ISpecification<QuizFavorite>>(), Arg.Any<CancellationToken>()).Returns(true);

        await _sut.Handle(new AddQuizFavoriteCommand(_quiz.Id), CancellationToken.None);

        await _favoriteRepository.DidNotReceive().AddAsync(Arg.Any<QuizFavorite>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenQuizNotFound_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _quizRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Quiz?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new AddQuizFavoriteCommand(missingId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new AddQuizFavoriteCommand(_quiz.Id), CancellationToken.None));
    }
}
