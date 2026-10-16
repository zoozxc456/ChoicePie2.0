using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class ListCreatorQuizzesQueryHandlerTests
{
    [Test]
    public async Task Handle_GivenCreatorId_WhenCalled_ThenReturnsCreatorsQuizzesFromQueryService()
    {
        var quizQueryService = Substitute.For<IQuizQueryService>();
        var creatorId = Guid.NewGuid();
        var expected = new PagedResult<QuizSummaryDto>([], 3, 5, 0);
        quizQueryService.ListByCreatorAsync(creatorId, 3, 5, Arg.Any<CancellationToken>()).Returns(expected);
        var sut = new ListCreatorQuizzesQueryHandler(quizQueryService);

        var result = await sut.Handle(new ListCreatorQuizzesQuery(creatorId, 3, 5), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }
}
