using ChoicePie.Backend.Application.QuizFavorites.Queries;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizFavorites;

[TestFixture]
public class GetQuizFavoriteStatusQueryHandlerTests
{
    private IQuizFavoriteRepository _favoriteRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetQuizFavoriteStatusQueryHandler _sut = null!;
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _favoriteRepository = Substitute.For<IQuizFavoriteRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetQuizFavoriteStatusQueryHandler(_favoriteRepository, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAnonymousUser_WhenCalled_ThenReturnsFalseWithoutQueryingRepository()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetQuizFavoriteStatusQuery(_quizId), CancellationToken.None);

        Assert.That(result, Is.False);
        await _favoriteRepository.DidNotReceive()
            .ExistsAsync(Arg.Any<ISpecification<ChoicePie.Backend.Domain.Aggregates.QuizFavorite.QuizFavorite>>(), Arg.Any<CancellationToken>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Handle_GivenAuthenticatedUser_WhenCalled_ThenReturnsRepositoryResult(bool exists)
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _favoriteRepository.ExistsAsync(Arg.Any<ISpecification<ChoicePie.Backend.Domain.Aggregates.QuizFavorite.QuizFavorite>>(), Arg.Any<CancellationToken>())
            .Returns(exists);

        var result = await _sut.Handle(new GetQuizFavoriteStatusQuery(_quizId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(exists));
    }
}
