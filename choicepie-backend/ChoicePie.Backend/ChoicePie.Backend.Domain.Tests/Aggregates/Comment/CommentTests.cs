using ChoicePie.Backend.Domain.Aggregates.Comment.Exceptions;
using CommentAggregate = ChoicePie.Backend.Domain.Aggregates.Comment.Comment;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Comment;

[TestFixture]
public class CommentTests
{
    [Test]
    public void Create_GivenValidInput_WhenCalled_ThenCreatesCommentWithExpectedFields()
    {
        var quizId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = CommentAggregate.Create(quizId, userId, "Great quiz!");

        Assert.Multiple(() =>
        {
            Assert.That(comment.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(comment.QuizId, Is.EqualTo(quizId));
            Assert.That(comment.UserId, Is.EqualTo(userId));
            Assert.That(comment.Text, Is.EqualTo("Great quiz!"));
        });
    }

    [Test]
    public void Create_GivenTextWithSurroundingWhitespace_WhenCalled_ThenTrimsText()
    {
        var comment = CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "  hello  ");

        Assert.That(comment.Text, Is.EqualTo("hello"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_GivenEmptyOrWhitespaceText_WhenCalled_ThenThrowsInvalidCommentTextException(string text)
    {
        Assert.Throws<InvalidCommentTextException>(() => CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), text));
    }

    [Test]
    public void Create_GivenTextExceedingMaxLength_WhenCalled_ThenThrowsInvalidCommentTextException()
    {
        var tooLong = new string('a', 1001);

        Assert.Throws<InvalidCommentTextException>(() => CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), tooLong));
    }

    [Test]
    public void UpdateText_GivenValidText_WhenCalled_ThenUpdatesTrimmedText()
    {
        var comment = CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "original");

        comment.UpdateText("  updated  ");

        Assert.That(comment.Text, Is.EqualTo("updated"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void UpdateText_GivenEmptyOrWhitespaceText_WhenCalled_ThenThrowsInvalidCommentTextException(string text)
    {
        var comment = CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "original");

        Assert.Throws<InvalidCommentTextException>(() => comment.UpdateText(text));
    }

    [Test]
    public void UpdateText_GivenTextExceedingMaxLength_WhenCalled_ThenThrowsInvalidCommentTextException()
    {
        var comment = CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "original");
        var tooLong = new string('a', 1001);

        Assert.Throws<InvalidCommentTextException>(() => comment.UpdateText(tooLong));
    }

    [Test]
    public void EnsureModifiableBy_GivenAuthor_WhenCalled_ThenDoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var comment = CommentAggregate.Create(Guid.NewGuid(), userId, "hello");

        Assert.DoesNotThrow(() => comment.EnsureModifiableBy(userId));
    }

    [Test]
    public void EnsureModifiableBy_GivenNonAuthor_WhenCalled_ThenThrowsCommentForbiddenException()
    {
        var comment = CommentAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");

        Assert.Throws<CommentForbiddenException>(() => comment.EnsureModifiableBy(Guid.NewGuid()));
    }
}
