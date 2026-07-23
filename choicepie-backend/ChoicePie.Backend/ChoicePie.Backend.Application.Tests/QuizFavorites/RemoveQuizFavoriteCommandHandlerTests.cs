using ChoicePie.Backend.Application.QuizFavorites.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizFavoriteAggregate = ChoicePie.Backend.Domain.Aggregates.QuizFavorite.QuizFavorite;

namespace ChoicePie.Backend.Application.Tests.QuizFavorites;

[TestFixture]
public class RemoveQuizFavoriteCommandHandlerTests
{
    private IQuizFavoriteRepository _favoriteRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RemoveQuizFavoriteCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _favoriteRepository = Substitute.For<IQuizFavoriteRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RemoveQuizFavoriteCommandHandler(_favoriteRepository, _currentUserService, _unitOfWork);

        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenExistingFavorite_WhenCalled_ThenDeletesAndPersists()
    {
        var favorite = QuizFavoriteAggregate.Create(_quizId, _userId);
        _favoriteRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<QuizFavoriteAggregate>>(), Arg.Any<CancellationToken>())
            .Returns(favorite);

        await _sut.Handle(new RemoveQuizFavoriteCommand(_quizId), CancellationToken.None);

        await _favoriteRepository.Received(1).DeleteAsync(favorite, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNoExistingFavorite_WhenCalled_ThenDoesNothing()
    {
        _favoriteRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<QuizFavoriteAggregate>>(), Arg.Any<CancellationToken>())
            .Returns((QuizFavoriteAggregate?)null);

        await _sut.Handle(new RemoveQuizFavoriteCommand(_quizId), CancellationToken.None);

        await _favoriteRepository.DidNotReceive().DeleteAsync(Arg.Any<QuizFavoriteAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new RemoveQuizFavoriteCommand(_quizId), CancellationToken.None));
    }
}
