using QuizFavoriteAggregate = ChoicePie.Backend.Domain.Aggregates.QuizFavorite.QuizFavorite;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.QuizFavorite;

[TestFixture]
public class QuizFavoriteTests
{
    [Test]
    public void Create_GivenQuizIdAndUserId_WhenCalled_ThenCreatesFavoriteWithExpectedFields()
    {
        var quizId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var favorite = QuizFavoriteAggregate.Create(quizId, userId);

        Assert.Multiple(() =>
        {
            Assert.That(favorite.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(favorite.QuizId, Is.EqualTo(quizId));
            Assert.That(favorite.UserId, Is.EqualTo(userId));
        });
    }
}
