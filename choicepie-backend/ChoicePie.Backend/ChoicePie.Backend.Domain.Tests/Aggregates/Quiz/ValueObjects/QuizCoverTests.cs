using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.ValueObjects;

[TestFixture]
public class QuizCoverTests
{
    [Test]
    public void Create_GivenValidColorKey_WhenCalled_ThenReturnsQuizCoverWithExpectedFields()
    {
        var cover = QuizCover.Create(null, "🚀", "primary");

        Assert.Multiple(() =>
        {
            Assert.That(cover.Emoji, Is.EqualTo("🚀"));
            Assert.That(cover.Gradient, Is.EqualTo("primary"));
        });
    }

    [Test]
    public void Create_GivenUnknownColorKey_WhenCalled_ThenThrowsInvalidQuizException()
    {
        Assert.Throws<InvalidQuizException>(() => QuizCover.Create(null, "🚀", "sunset"));
    }

    [Test]
    public void Create_GivenNullImageUrl_WhenCalled_ThenReturnsQuizCoverWithNullImageUrl()
    {
        var cover = QuizCover.Create(null, "🚀", "primary");

        Assert.That(cover.ImageUrl, Is.Null);
    }

    [Test]
    public void Create_GivenBlankEmoji_WhenCalled_ThenThrowsInvalidQuizException()
    {
        Assert.Throws<InvalidQuizException>(() => QuizCover.Create(null, "   ", "primary"));
    }
}
