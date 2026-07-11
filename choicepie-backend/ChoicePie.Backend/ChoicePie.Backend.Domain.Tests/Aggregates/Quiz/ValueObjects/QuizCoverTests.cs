using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.ValueObjects;

[TestFixture]
public class QuizCoverTests
{
    [Test]
    public void Create_GivenEmojiAndGradient_WhenCalled_ThenReturnsQuizCoverWithExpectedFields()
    {
        var cover = QuizCover.Create("🚀", "sunset");

        Assert.Multiple(() =>
        {
            Assert.That(cover.Emoji, Is.EqualTo("🚀"));
            Assert.That(cover.Gradient, Is.EqualTo("sunset"));
        });
    }
}
