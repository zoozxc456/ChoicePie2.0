using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.ExternalServices.Quizzes;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.Quizzes;

[TestFixture]
public class PlaceholderQuizGenerationServiceTests
{
    private PlaceholderQuizGenerationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new PlaceholderQuizGenerationService();
    }

    [Test]
    public async Task GenerateAsync_GivenRequestedQuestionCount_WhenCalled_ThenReturnsThatManyQuestions()
    {
        var result = await _sut.GenerateAsync("some content", 5, Difficulty.Beginner, CancellationToken.None);

        Assert.That(result.Questions, Has.Count.EqualTo(5));
    }
}
